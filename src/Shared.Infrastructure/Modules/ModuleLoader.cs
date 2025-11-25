using System.Reflection;
using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Modules;

/// <summary>
/// Discovers and loads IModule implementations.
/// Each module is responsible for its own registration logic via Register() and Use() methods.
/// </summary>
public static class ModuleLoader
{
    /// <summary>
    /// Default assembly prefixes to exclude from scanning (System, Microsoft, etc).
    /// </summary>
    private static readonly string[] DefaultExclusionPrefixes = new[]
    {
        "System.",
        "Microsoft.",
        "netstandard"
    };

    /// <summary>
    /// Discovers all IModule implementations from the specified assemblies and instantiates them.
    /// </summary>
    /// <param name="assembliesToScan">Assemblies to scan for modules. If null, scans all AppDomain assemblies (filtered by exclusion prefixes).</param>
    /// <param name="exclusionPrefixes">Assembly name prefixes to exclude from scanning (in addition to default exclusions). Example: new[] { "Legacy.", "Old." }</param>
    /// <returns>List of discovered module instances.</returns>
    public static IList<IModule> LoadModules(Assembly[]? assembliesToScan = null, string[]? exclusionPrefixes = null)
    {
        // Determine which assemblies to scan
        var allExclusionPrefixes = (exclusionPrefixes ?? Array.Empty<string>())
            .Concat(DefaultExclusionPrefixes)
            .ToArray();

        var assemblies = (assembliesToScan ?? AppDomain.CurrentDomain.GetAssemblies())
            .Where(a => !ShouldExcludeAssembly(a, allExclusionPrefixes))
            .ToList();

        // Collect all types from filtered assemblies
        var types = new List<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                types.AddRange(assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (ex.Types != null)
                    types.AddRange(ex.Types.Where(t => t != null)!);
            }
            catch
            {
                // Skip assemblies that can't be loaded
            }
        }

        // Find and instantiate IModule implementations
        var modules = new List<IModule>();
        foreach (var type in types
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .OrderBy(t => t.Name))
        {
            if (Activator.CreateInstance(type) is IModule module)
                modules.Add(module);
        }

        return modules;
    }

    /// <summary>
    /// Determines whether an assembly should be excluded from scanning.
    /// </summary>
    private static bool ShouldExcludeAssembly(Assembly assembly, string[] exclusionPrefixes)
    {
        var assemblyName = assembly.GetName().Name;
        if (string.IsNullOrEmpty(assemblyName))
            return true;

        return exclusionPrefixes.Any(prefix =>
            assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
