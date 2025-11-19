using System.Reflection;
using System.Text;
using Shared.Abstractions.Options;

namespace Shared.EnvFileGenerator.Services;

/// <summary>
/// Service responsible for scanning .NET project assemblies and generating .env files.
/// Scans for classes implementing IOptions interface from Shared.Abstractions.Options.
///
/// Features:
/// - Scans bin folder of project for compiled assemblies
/// - Searches for classes implementing IOptions interface
/// - Extracts SectionName from static property
/// - Generates environment variables in SECTION__PROPERTY_NAME format
/// - Deduplicates types to avoid duplicate sections
/// </summary>
public class EnvFileGenerator
{

    /// <summary>
    /// Generates an .env file by scanning the project's bin folder.
    /// </summary>
    /// <param name="projectPath">Path to the project to scan</param>
    /// <param name="outputPath">Output .env file path</param>
    /// <param name="recursive">Whether to scan referenced assemblies</param>
    /// <param name="includeDescriptions">Include comments with descriptions</param>
    /// <param name="overwrite">Overwrite existing file without prompting</param>
    /// <param name="config">Build configuration (Debug/Release)</param>
    /// <param name="cancellationToken">Cancellation token</param>
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

        // Resolve project path to absolute
        var absoluteProjectPath = Path.GetFullPath(projectPath);
        var binFolder = Path.Combine(absoluteProjectPath, "bin", config);

        if (!Directory.Exists(binFolder))
        {
            Console.WriteLine($"Error: Build output folder not found: {binFolder}");
            Console.WriteLine($"Make sure to build the project first: dotnet build");
            return;
        }

        Console.WriteLine($"Scanning assemblies in {binFolder}...");

        var optionsTypes = new List<Type>();
        var scannedAssemblies = new HashSet<string>();

        // Find all DLL files in bin folder
        var dlls = Directory.GetFiles(binFolder, "*.dll", SearchOption.AllDirectories);

        foreach (var dll in dlls)
        {
            try
            {
                var types = ScanAssembly(dll, recursive, scannedAssemblies);
                optionsTypes.AddRange(types);
            }
            catch (Exception ex)
            {
                // Skip assemblies that can't be loaded
                Console.WriteLine($"  Warning: Could not load {Path.GetFileName(dll)}: {ex.Message}");
            }
        }

        if (optionsTypes.Count == 0)
        {
            Console.WriteLine("No IOptions implementations found.");
            return;
        }

        Console.WriteLine($"Found {optionsTypes.Count} configuration section(s).");

        var content = GenerateEnvContent(optionsTypes, includeDescriptions);

        // Resolve output path to absolute if relative
        var absoluteOutputPath = Path.IsPathRooted(outputPath)
            ? outputPath
            : Path.Combine(absoluteProjectPath, outputPath);

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(absoluteOutputPath);
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir!);
        }

        await File.WriteAllTextAsync(absoluteOutputPath, content, cancellationToken);

        Console.WriteLine($"Generated {absoluteOutputPath} successfully.");
    }

    /// <summary>
    /// Scans a single assembly for IOptions implementations.
    /// </summary>
    private List<Type> ScanAssembly(string assemblyPath, bool recursive, HashSet<string> scannedAssemblies)
    {
        var result = new List<Type>();
        var fullPath = Path.GetFullPath(assemblyPath);

        if (scannedAssemblies.Contains(fullPath))
            return result;

        scannedAssemblies.Add(fullPath);

        try
        {
            // Set up assembly resolution for dependencies
            var assemblyDir = Path.GetDirectoryName(fullPath)!;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name).Name;
                var dependencyPath = Path.Combine(assemblyDir, assemblyName + ".dll");
                if (File.Exists(dependencyPath))
                {
                    return Assembly.LoadFrom(dependencyPath);
                }
                return null;
            };

            var assembly = Assembly.LoadFrom(fullPath);
            Console.WriteLine($"  Scanning: {assembly.GetName().Name}");

            // Find types implementing IOptions
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Get the types that were successfully loaded
                types = ex.Types.Where(t => t != null).ToArray()!;
                Console.WriteLine($"    Warning: Some types could not be loaded, scanning {types.Length} available types");
            }

            // Look for classes implementing IOptions interface
            var optionsTypes = types
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                            t.GetInterfaces().Any(i => i == typeof(IOptions)));

            foreach (var type in optionsTypes)
            {
                Console.WriteLine($"    Found: {type.Name}");
                result.Add(type);
            }

            // Recursively scan referenced assemblies
            if (recursive)
            {
                foreach (var reference in assembly.GetReferencedAssemblies())
                {
                    var refPath = Path.Combine(assemblyDir, reference.Name + ".dll");
                    if (File.Exists(refPath))
                    {
                        var refTypes = ScanAssembly(refPath, recursive, scannedAssemblies);
                        result.AddRange(refTypes);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Could not load {Path.GetFileName(assemblyPath)}: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Generates the .env file content from discovered IOptions types.
    /// Deduplicates by full type name, sorts by section name, and formats each property
    /// as SECTION__PROPERTY_NAME with optional comments and metadata.
    /// </summary>
    private string GenerateEnvContent(List<Type> optionsTypes, bool includeDescriptions)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Environment Variables Configuration");
        sb.AppendLine($"# Generated by Shared.Tools on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Deduplicate by full type name to avoid duplicates from recursive scanning
        var uniqueTypes = optionsTypes
            .GroupBy(t => t.FullName)
            .Select(g => g.First())
            .ToList();

        foreach (var type in uniqueTypes.OrderBy(t => GetSectionName(t)))
        {
            var sectionName = GetSectionName(type);

            sb.AppendLine($"# ==========================================");
            sb.AppendLine($"# {sectionName}");
            sb.AppendLine($"# ==========================================");

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .OrderBy(p => p.Name);

            foreach (var property in properties)
            {
                var envName = GetEnvVariableName(sectionName, property);
                var defaultValue = GetDefaultValue(property.PropertyType);

                if (includeDescriptions)
                {
                    sb.AppendLine($"# Type: {GetTypeName(property.PropertyType)}");
                }

                sb.AppendLine($"{envName}={defaultValue}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the configuration section name from a type.
    /// Priority: static SectionName property > class name without "Options" suffix
    /// </summary>
    private string GetSectionName(Type type)
    {
        // Check for static SectionName property (IOptions interface requirement)
        var sectionNameProperty = type.GetProperty("SectionName", BindingFlags.Public | BindingFlags.Static);
        if (sectionNameProperty != null)
        {
            var value = sectionNameProperty.GetValue(null)?.ToString();
            if (!string.IsNullOrEmpty(value))
                return value;
        }

        // Fallback: class name without "Options" suffix
        return type.Name.Replace("Options", "");
    }

    /// <summary>
    /// Gets the environment variable name for a property.
    /// Generates SECTIONNAME__PROPERTYNAME format (uppercase without word separators).
    /// This matches .NET configuration's environment variable naming convention.
    /// </summary>
    private string GetEnvVariableName(string sectionName, PropertyInfo property)
    {
        // Convert to uppercase without adding underscores between words
        // Example: "SmtpSettings" + "Host" -> "SMTPSETTINGS__HOST"
        var propertyName = property.Name.ToUpperInvariant();
        var section = sectionName.ToUpperInvariant();

        // Use double underscore as section separator (standard .NET configuration pattern)
        return $"{section}__{propertyName}";
    }

    private string GetDefaultValue(Type type)
    {
        if (type == typeof(string))
            return "";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
            return "0";
        if (type == typeof(bool))
            return "false";
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
            return "0.0";
        if (type == typeof(Guid))
            return "00000000-0000-0000-0000-000000000000";
        if (type == typeof(TimeSpan))
            return "00:00:00";
        if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            return "";

        return "";
    }

    private string GetTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(short)) return "short";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(Guid)) return "Guid";
        if (type == typeof(TimeSpan)) return "TimeSpan";
        if (type.IsArray) return $"{GetTypeName(type.GetElementType()!)}[]";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return $"List<{GetTypeName(type.GetGenericArguments()[0])}>";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return $"{GetTypeName(type.GetGenericArguments()[0])}?";

        return type.Name;
    }
}
