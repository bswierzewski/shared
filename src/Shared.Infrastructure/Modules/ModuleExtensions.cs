using Microsoft.AspNetCore.Builder;
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
        // Load modules from assemblies ONCE
        var assemblies = ModuleLoader.LoadAssemblies(configuration);
        var modules = ModuleLoader.LoadModules(assemblies, configuration);

        // Register modules collection in DI container (singleton)
        // This allows UseModules() and InitializeApplicationAsync() to access modules without reloading
        services.AddSingleton<IReadOnlyCollection<IModule>>(modules.AsReadOnly());

        // Add MediatR with behaviors and handlers from all modules
        services.AddMediatRWithBehaviors(modules);

        // Add FluentValidation validators from all modules
        services.AddValidatorsFromModules(modules);

        // Add EF Core interceptors
        services.AddAuditableEntityInterceptor();
        services.AddDomainEventDispatchInterceptor();

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
    /// Retrieves modules from the DI container (loaded once during AddModules).
    /// Invokes the Use method on all registered modules.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseModules(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        // Get modules from DI container (already loaded in AddModules)
        var modules = app.ApplicationServices.GetRequiredService<IReadOnlyCollection<IModule>>();

        // Configure middleware for each module
        foreach (var module in modules)
        {
            module.Use(app, configuration);
        }

        return app;
    }

    #endregion

    #region Application Initialization

    /// <summary>
    /// Initializes all registered modules.
    /// Retrieves modules from the DI container (loaded once during AddModules).
    /// Calls the Initialize method on each module to perform startup tasks like running migrations and seeding data.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public static async Task InitializeApplicationAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        // Get modules from DI container (already loaded in AddModules)
        var modules = serviceProvider.GetRequiredService<IReadOnlyCollection<IModule>>();

        // Initialize each module
        foreach (var module in modules)
        {
            await module.Initialize(serviceProvider, cancellationToken);
        }
    }

    #endregion
}
