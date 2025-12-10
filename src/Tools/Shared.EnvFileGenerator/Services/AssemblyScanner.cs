#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Reflection;
using Shared.Abstractions.Options;

namespace Shared.EnvFileGenerator.Services;

/// <summary>
/// Scans .NET assemblies to discover IOptions implementations.
/// </summary>
internal class AssemblyScanner
{
    private static readonly string[] ExcludedPrefixes =
    [
        "System.", "Microsoft.", "Windows.", "mscorlib", "netstandard",
        "api-ms-", "testhost", "xunit", "nunit", "JetBrains"
    ];

    /// <summary>
    /// Loads all IOptions types from the project's bin folder.
    /// </summary>
    public List<Type> LoadOptionsTypes(string projectPath, string config, bool recursive)
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
            try
            {
                var types = ScanAssembly(dll, recursive, scannedAssemblies);
                optionsTypes.AddRange(types);
            }
            catch (Exception ex)
            {
                if (!ShouldSkipAssembly(dll))
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
        if (ShouldSkipAssembly(assemblyPath)) return [];

        var fullPath = Path.GetFullPath(assemblyPath);

        if (scannedAssemblies.Contains(fullPath)) return [];
        scannedAssemblies.Add(fullPath);

        var result = new List<Type>();

        try
        {
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

                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }

                var matchingTypes = types
                    .Where(t => t is { IsClass: true, IsAbstract: false } &&
                                t.GetInterfaces().Any(i => i == typeof(IOptions)));

                foreach (var type in matchingTypes)
                {
                    Console.WriteLine($"    Found: {type.Name}");
                    result.Add(type);
                }

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
}
