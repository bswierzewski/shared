using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Extensions.Internal;

/// <summary>
/// Internal extension methods for module registration and configuration.
/// </summary>
internal static class ModuleRegistrationExtensions
{
    /// <summary>
    /// Registers services from each module.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modules">The list of modules to register.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection RegisterModules(
        this IServiceCollection services,
        IList<IModule> modules)
    {
        foreach (var module in modules)
        {
            Console.WriteLine($"Registering module '{module.Name}'...");
            module.Register(services);
        }

        return services;
    }
}
