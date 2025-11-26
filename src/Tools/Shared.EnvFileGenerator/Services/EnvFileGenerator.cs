using System.Reflection;
using System.Text;
using Shared.Abstractions.Options;

namespace Shared.EnvFileGenerator.Services;

/// <summary>
/// Service responsible for scanning .NET project assemblies and generating .env files.
/// </summary>
public class EnvFileGenerator
{
    private static readonly string[] ExcludedPrefixes =
    [
        "System.", "Microsoft.", "Windows.", "mscorlib", "netstandard",
        "api-ms-", "testhost", "xunit", "nunit", "JetBrains"
    ];

    /// <summary>
    /// Generates an .env file by scanning the project's bin folder.
    /// </summary>
    public async Task GenerateAsync(
        string projectPath,
        string outputPath,
        bool recursive,
        bool includeDescriptions,
        bool overwrite,
        string config,
        CancellationToken cancellationToken = default)
    {
        if (File.Exists(outputPath) && !overwrite)
        {
            Console.WriteLine($"File {outputPath} already exists. Use --force to overwrite.");
            return;
        }

        // 1. Load types using the centralized method
        var optionsTypes = LoadOptionsTypes(projectPath, config, recursive);
        if (optionsTypes.Count == 0) return;

        // 2. Generate content
        var content = GenerateEnvContent(optionsTypes, includeDescriptions);

        // 3. Save file
        var absoluteOutputPath = ResolvePath(projectPath, outputPath);
        EnsureDirectoryExists(absoluteOutputPath);

        await File.WriteAllTextAsync(absoluteOutputPath, content, cancellationToken);
        Console.WriteLine($"Generated {absoluteOutputPath} successfully.");
    }

    /// <summary>
    /// Lists all available configuration sections from IOptions implementations.
    /// </summary>
    public async Task ListSectionsAsync(
        string projectPath,
        bool recursive,
        bool verbose,
        string config,
        CancellationToken cancellationToken = default)
    {
        // 1. Load types using the centralized method
        var optionsTypes = LoadOptionsTypes(projectPath, config, recursive);
        if (optionsTypes.Count == 0) return;

        Console.WriteLine($"\nFound {optionsTypes.Count} configuration section(s):\n");

        // 2. Display
        var uniqueTypes = DeduplicateTypes(optionsTypes);

        foreach (var type in uniqueTypes)
        {
            var sectionName = GetSectionName(type);
            var properties = GetConfigProperties(type);

            Console.WriteLine($"[{sectionName}]");
            if (verbose)
            {
                foreach (var property in properties)
                {
                    var envName = GetEnvVariableName(sectionName, property);
                    Console.WriteLine($"  {envName} ({GetTypeName(property.PropertyType)})");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"  Properties: {properties.Count}\n");
            }
        }

        await Task.CompletedTask; // Keep signature async compatible
    }

    /// <summary>
    /// Updates an existing .env file with new configuration sections.
    /// </summary>
    public async Task UpdateAsync(
        string projectPath,
        string envFilePath,
        bool recursive,
        bool includeDescriptions,
        bool backup,
        string config,
        CancellationToken cancellationToken = default)
    {
        // 1. Load types using the centralized method
        var optionsTypes = LoadOptionsTypes(projectPath, config, recursive);
        if (optionsTypes.Count == 0) return;

        var absoluteEnvPath = ResolvePath(projectPath, envFilePath);

        // 2. Read existing
        var existingValues = new Dictionary<string, string>();
        if (File.Exists(absoluteEnvPath))
        {
            Console.WriteLine($"Reading existing .env file...");
            if (backup)
            {
                File.Copy(absoluteEnvPath, $"{absoluteEnvPath}.backup", overwrite: true);
                Console.WriteLine($"Backup created: {absoluteEnvPath}.backup");
            }

            existingValues = await ParseEnvFile(absoluteEnvPath, cancellationToken);
        }

        // 3. Merge and Save
        var newContent = GenerateEnvContent(optionsTypes, includeDescriptions);
        var mergedContent = MergeEnvContent(newContent, existingValues);

        EnsureDirectoryExists(absoluteEnvPath);
        await File.WriteAllTextAsync(absoluteEnvPath, mergedContent, cancellationToken);
        Console.WriteLine($"Updated {absoluteEnvPath} successfully.");
    }

    // =================================================================================================
    // CORE LOGIC (Scanning & Loading)
    // =================================================================================================

    /// <summary>
    /// Centralized method to find the bin folder, gather DLLs, and scan them for IOptions.
    /// </summary>
    private List<Type> LoadOptionsTypes(string projectPath, string config, bool recursive)
    {
        var absoluteProjectPath = Path.GetFullPath(projectPath);
        var binFolder = Path.Combine(absoluteProjectPath, "bin", config);

        if (!Directory.Exists(binFolder))
        {
            Console.WriteLine($"Error: Build output folder not found: {binFolder}");
            Console.WriteLine($"Make sure to build the project first: dotnet build");
            return [];
        }

        Console.WriteLine($"Scanning assemblies in {binFolder}...");

        var optionsTypes = new List<Type>();
        var scannedAssemblies = new HashSet<string>();
        var dlls = Directory.GetFiles(binFolder, "*.dll", SearchOption.AllDirectories);

        foreach (var dll in dlls)
        {
            // We just pass the path. skip logic is handled internally in ScanAssembly
            try
            {
                var types = ScanAssembly(dll, recursive, scannedAssemblies);
                optionsTypes.AddRange(types);
            }
            catch (Exception ex)
            {
                if (!ShouldSkipAssembly(dll)) // Only log warnings for user code
                    Console.WriteLine($"  Warning: Could not load {Path.GetFileName(dll)}: {ex.Message}");
            }
        }

        if (optionsTypes.Count == 0)
        {
            Console.WriteLine("No IOptions implementations found.");
        }
        else
        {
            Console.WriteLine($"Found {optionsTypes.Count} configuration section(s).");
        }

        return optionsTypes;
    }

    private List<Type> ScanAssembly(string assemblyPath, bool recursive, HashSet<string> scannedAssemblies)
    {
        // 1. Guard Clause: Skip unwanted assemblies immediately
        if (ShouldSkipAssembly(assemblyPath)) return [];

        var fullPath = Path.GetFullPath(assemblyPath);

        // 2. Guard Clause: Already visited
        if (scannedAssemblies.Contains(fullPath)) return [];
        scannedAssemblies.Add(fullPath);

        var result = new List<Type>();

        try
        {
            // Setup dependency resolver
            var assemblyDir = Path.GetDirectoryName(fullPath)!;
            ResolveEventHandler resolver = (sender, args) =>
            {
                var name = new AssemblyName(args.Name).Name;
                if (ShouldSkipAssembly(name!)) return null;

                var depPath = Path.Combine(assemblyDir, name + ".dll");
                return File.Exists(depPath) ? Assembly.LoadFrom(depPath) : null;
            };

            AppDomain.CurrentDomain.AssemblyResolve += resolver;

            try
            {
                var assembly = Assembly.LoadFrom(fullPath);
                Console.WriteLine($"  Scanning: {assembly.GetName().Name}");

                // Get Types safely
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }

                // Filter IOptions
                var matchingTypes = types
                    .Where(t => t is { IsClass: true, IsAbstract: false } &&
                                t.GetInterfaces().Any(i => i == typeof(IOptions)));

                foreach (var type in matchingTypes)
                {
                    Console.WriteLine($"    Found: {type.Name}");
                    result.Add(type);
                }

                // Recursion
                if (recursive)
                {
                    foreach (var reference in assembly.GetReferencedAssemblies())
                    {
                        if (ShouldSkipAssembly(reference.Name!)) continue;

                        var refPath = Path.Combine(assemblyDir, reference.Name + ".dll");
                        if (File.Exists(refPath))
                        {
                            result.AddRange(ScanAssembly(refPath, recursive, scannedAssemblies));
                        }
                    }
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolver;
            }
        }
        catch (Exception ex)
        {
            // Log only if it's potentially relevant
            if (!ShouldSkipAssembly(assemblyPath))
                Console.WriteLine($"  Warning: Failed to scan {Path.GetFileName(assemblyPath)}: {ex.Message}");
        }

        return result;
    }

    private bool ShouldSkipAssembly(string pathOrName)
    {
        var fileName = Path.GetFileName(pathOrName);
        if (fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            fileName = Path.GetFileNameWithoutExtension(fileName);

        return ExcludedPrefixes.Any(prefix =>
            fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    // =================================================================================================
    // HELPERS (Content Generation & File I/O)
    // =================================================================================================

    private string GenerateEnvContent(List<Type> optionsTypes, bool includeDescriptions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Environment Variables Configuration");
        sb.AppendLine($"# Generated by Shared.Tools on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        var uniqueTypes = DeduplicateTypes(optionsTypes);

        foreach (var type in uniqueTypes)
        {
            var sectionName = GetSectionName(type);

            sb.AppendLine($"# ==========================================");
            sb.AppendLine($"# {sectionName}");
            sb.AppendLine($"# ==========================================");

            foreach (var property in GetConfigProperties(type))
            {
                var envName = GetEnvVariableName(sectionName, property);
                var defaultValue = GetDefaultValue(property.PropertyType);

                if (includeDescriptions)
                    sb.AppendLine($"# Type: {GetTypeName(property.PropertyType)}");

                sb.AppendLine($"{envName}={defaultValue}");
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    private List<Type> DeduplicateTypes(List<Type> types)
        => types.GroupBy(t => t.FullName).Select(g => g.First()).OrderBy(t => GetSectionName(t)).ToList();

    private List<PropertyInfo> GetConfigProperties(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
               .Where(p => p.CanRead && p.CanWrite)
               .OrderBy(p => p.Name)
               .ToList();

    private string MergeEnvContent(string newContent, Dictionary<string, string> existingValues)
    {
        var sb = new StringBuilder();
        foreach (var line in newContent.Split(Environment.NewLine))
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#"))
            {
                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2 && existingValues.TryGetValue(parts[0], out var existingVal))
                {
                    sb.AppendLine($"{parts[0]}={existingVal}");
                    continue;
                }
            }
            sb.AppendLine(line);
        }
        return sb.ToString();
    }

    private async Task<Dictionary<string, string>> ParseEnvFile(string path, CancellationToken ct)
    {
        var result = new Dictionary<string, string>();
        var lines = await File.ReadAllLinesAsync(path, ct);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2) result[parts[0]] = parts[1];
        }
        return result;
    }

    private string ResolvePath(string projectPath, string relativePath)
    {
        var absoluteProjectPath = Path.GetFullPath(projectPath);
        return Path.IsPathRooted(relativePath) ? relativePath : Path.Combine(absoluteProjectPath, relativePath);
    }

    private void EnsureDirectoryExists(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
    }

    private string GetSectionName(Type type)
    {
        var prop = type.GetProperty("SectionName", BindingFlags.Public | BindingFlags.Static);
        return prop?.GetValue(null)?.ToString() ?? type.Name.Replace("Options", "");
    }

    private string GetEnvVariableName(string section, PropertyInfo prop)
        => $"{section.ToUpperInvariant().Replace(":", "__")}__{prop.Name.ToUpperInvariant()}";

    private string GetTypeName(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return $"{GetTypeName(type.GetGenericArguments()[0])}?";
        if (type.IsArray) return $"{GetTypeName(type.GetElementType()!)}[]";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return $"List<{GetTypeName(type.GetGenericArguments()[0])}>";

        return type.Name switch
        {
            "String" => "string",
            "Int32" => "int",
            "Int64" => "long",
            "Boolean" => "bool",
            "Double" => "double",
            "Decimal" => "decimal",
            _ => type.Name
        };
    }

    private string GetDefaultValue(Type type)
    {
        if (type == typeof(string)) return "";
        if (type == typeof(bool)) return "false";
        if (type.IsValueType && type != typeof(Guid) && type != typeof(TimeSpan)) return "0";
        if (type == typeof(Guid)) return Guid.Empty.ToString();
        if (type == typeof(TimeSpan)) return "00:00:00";
        return "";
    }
}