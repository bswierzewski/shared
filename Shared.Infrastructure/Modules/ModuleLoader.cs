using System.Reflection;
using Microsoft.Extensions.Configuration;
using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Modules;

/// <summary>
/// Provides functionality for dynamically loading modules from assemblies.
/// Modules can be enabled or disabled via configuration.
/// </summary>
public static class ModuleLoader
{
    /// <summary>
    /// Loads assemblies containing modules, filtering out disabled modules based on configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="modulePart">The namespace part that identifies module assemblies (default: "Modules").</param>
    /// <returns>List of loaded assemblies.</returns>
    public static IList<Assembly> LoadAssemblies(IConfiguration configuration, string modulePart = "Modules")
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var locations = assemblies
            .Where(x => !x.IsDynamic)
            .Select(x => x.Location)
            .ToArray();

        var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
            .Where(x => !locations.Contains(x, StringComparer.InvariantCultureIgnoreCase))
            .ToList();

        var disabledModules = new List<string>();

        foreach (var file in files)
        {
            if (!file.Contains(modulePart))
            {
                continue;
            }

            var moduleName = ExtractModuleName(file, modulePart);
            if (string.IsNullOrEmpty(moduleName))
            {
                continue;
            }

            var enabled = configuration.GetValue<bool>($"{moduleName}:module:enabled", true);
            if (!enabled)
            {
                disabledModules.Add(file);
            }
        }

        foreach (var file in files.Except(disabledModules))
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(file);
                var assembly = AppDomain.CurrentDomain.Load(assemblyName);
                assemblies.Add(assembly);
            }
            catch (BadImageFormatException)
            {
                // Skip non-.NET assemblies
            }
            catch (Exception)
            {
                // Skip assemblies that cannot be loaded
            }
        }

        return assemblies;
    }

    /// <summary>
    /// Discovers and instantiates all IModule implementations from the given assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for modules.</param>
    /// <returns>List of module instances.</returns>
    public static IList<IModule> LoadModules(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IModule).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .OrderBy(x => x.Name)
            .Select(x =>
            {
                try
                {
                    return Activator.CreateInstance(x) as IModule;
                }
                catch
                {
                    return null;
                }
            })
            .Where(x => x is not null)
            .Cast<IModule>()
            .ToList();
    }

    /// <summary>
    /// Discovers and instantiates all IModule implementations from the given assemblies,
    /// filtering out disabled modules based on configuration.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for modules.</param>
    /// <param name="configuration">The application configuration to check for enabled modules.</param>
    /// <returns>List of enabled module instances.</returns>
    public static IList<IModule> LoadModules(IEnumerable<Assembly> assemblies, IConfiguration configuration)
    {
        return assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IModule).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .OrderBy(x => x.Name)
            .Select(x =>
            {
                try
                {
                    return Activator.CreateInstance(x) as IModule;
                }
                catch
                {
                    return null;
                }
            })
            .Where(x => x is not null)
            .Cast<IModule>()
            .Where(module => configuration.GetValue($"{module.Name}:module:enabled", true))
            .ToList();
    }

    /// <summary>
    /// Extracts the module name from an assembly file path.
    /// </summary>
    /// <param name="filePath">The full path to the assembly file.</param>
    /// <param name="modulePart">The namespace part that identifies module assemblies.</param>
    /// <returns>The module name in lowercase, or empty string if not found.</returns>
    private static string ExtractModuleName(string filePath, string modulePart)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var parts = fileName.Split('.');

        var moduleIndex = Array.FindIndex(parts, p =>
            p.Equals(modulePart, StringComparison.OrdinalIgnoreCase));

        if (moduleIndex >= 0 && moduleIndex + 1 < parts.Length)
        {
            return parts[moduleIndex + 1].ToLowerInvariant();
        }

        return string.Empty;
    }
}
