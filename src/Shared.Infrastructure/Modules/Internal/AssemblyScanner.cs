using System.Reflection;
using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Modules.Internal;

/// <summary>
/// Internal utility for scanning assemblies marked with IModuleAssembly.
/// Provides shared logic for MediatR, FluentValidation, and Endpoint discovery.
/// </summary>
internal static class AssemblyScanner
{
    /// <summary>
    /// Scans all loaded assemblies and returns those marked with IModuleAssembly.
    /// Assemblies with IModuleAssembly marker will be registered for:
    /// - MediatR handlers (IRequestHandler, INotificationHandler)
    /// - FluentValidation validators (IValidator, AbstractValidator)
    /// - Module endpoints (IModuleEndpoints)
    /// </summary>
    /// <returns>Collection of assemblies marked with IModuleAssembly</returns>
    internal static HashSet<Assembly> GetModuleAssemblies()
    {
        var moduleAssemblies = new HashSet<Assembly>();
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic);

        foreach (var assembly in allAssemblies)
        {
            try
            {
                // Check if assembly contains any type implementing IModuleAssembly
                var hasModuleMarker = assembly.GetTypes()
                    .Any(t => typeof(IModuleAssembly).IsAssignableFrom(t)
                           && !t.IsInterface
                           && !t.IsAbstract);

                if (hasModuleMarker)
                    moduleAssemblies.Add(assembly);
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that can't be loaded due to missing dependencies
                // This is expected for some third-party assemblies
            }
        }

        return moduleAssemblies;
    }
}
