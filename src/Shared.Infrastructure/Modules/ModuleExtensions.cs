using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Modules.Internal;
using Shared.Infrastructure.Persistence.Migrations;

namespace Shared.Infrastructure.Modules;

/// <summary>
/// Extension methods for module registration and configuration.
/// </summary>
public static class ModuleExtensions
{
    #region AddModules

    /// <summary>
    /// Adds all module infrastructure services to the service collection.
    /// This includes module discovery, registration, MediatR with behaviors, validators, and interceptors.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Load modules from assemblies
        var assemblies = ModuleLoader.LoadAssemblies(configuration);
        var modules = ModuleLoader.LoadModules(assemblies, configuration);

        // Register module information
        AddModuleInfo(services, modules);

        // Add MediatR with behaviors and handlers from all modules
        services.AddMediatRWithBehaviors(modules);

        // Add FluentValidation validators from all modules
        services.AddValidatorsFromModules(modules);

        // Add EF Core interceptors
        services.AddAuditableEntityInterceptor();
        services.AddDomainEventDispatchInterceptor();

        // Register services from each module
        RegisterModules(services, modules, configuration);

        return services;
    }

    /// <summary>
    /// Registers module information and metadata with the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modules">The list of loaded modules.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddModuleInfo(this IServiceCollection services, IList<IModule> modules)
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
    /// Registers services from each module.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modules">The list of modules to register.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection RegisterModules(
        this IServiceCollection services,
        IList<IModule> modules,
        IConfiguration configuration)
    {
        foreach (var module in modules)
        {
            Console.WriteLine($"Registering module '{module.Name}'...");
            module.Register(services, configuration);
        }

        return services;
    }

    #endregion

    #region UseModules

    /// <summary>
    /// Configures the application pipeline for all loaded modules.
    /// Discovers and invokes the Use method on all registered modules.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseModules(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        // Load modules from assemblies
        var assemblies = ModuleLoader.LoadAssemblies(configuration);
        var modules = ModuleLoader.LoadModules(assemblies, configuration);

        // Configure middleware for each module
        foreach (var module in modules)
        {
            module.Use(app, configuration);
        }

        return app;
    }

    #endregion

    #region Persistence

    /// <summary>
    /// Adds migration service for the specified DbContext.
    /// Runs on application startup to check for and apply pending migrations.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMigrationService<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        return services.AddHostedService<MigrationService<TContext>>();
    }

    #endregion
}
