using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Modules;

/// <summary>
/// Extension methods for module registration and configuration.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Registers module information and metadata with the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modules">The list of loaded modules.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModuleInfo(this IServiceCollection services, IList<IModule> modules)
    {
        var moduleRegistry = new ModuleRegistry();

        foreach (var module in modules)
        {
            var permissions = module.GetPermissions().ToList().AsReadOnly();
            var roles = module.GetRoles().ToList().AsReadOnly();

            var moduleInfo = new ModuleInfo(
                module.Name,
                permissions,
                roles);

            moduleRegistry.Register(moduleInfo);
        }

        services.AddSingleton<IModuleRegistry>(moduleRegistry);

        return services;
    }

    /// <summary>
    /// Invokes the Use method on all registered modules to configure their middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="modules">The list of modules to configure.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseModules(this IApplicationBuilder app, IList<IModule> modules)
    {
        foreach (var module in modules)
        {
            module.Use(app);
        }

        return app;
    }
}
