using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Modules;

/// <summary>
/// Extension methods for module registration and middleware configuration.
/// Provides simple loops for RegisterModules and UseModules that each module implements.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Registers all modules by calling their Register() method.
    /// Each module is responsible for registering its own services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modules">The modules to register.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection RegisterModules(
        this IServiceCollection services,
        IEnumerable<IModule> modules,
        IConfiguration configuration)
    {
        foreach (var module in modules)
        {
            Console.WriteLine($"Registering module '{module.Name}'...");
            module.Register(services, configuration);
        }

        return services;
    }

    /// <summary>
    /// Configures middleware for all modules by calling their Use() method.
    /// Each module is responsible for configuring its own middleware and endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="modules">The modules to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseModules(
        this IApplicationBuilder app,
        IEnumerable<IModule> modules,
        IConfiguration configuration)
    {
        foreach (var module in modules)
        {
            module.Use(app, configuration);
        }

        return app;
    }

    /// <summary>
    /// Initializes all modules by calling their Initialize() method.
    /// Each module is responsible for its own initialization tasks (migrations, seeding, etc).
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="modules">The modules to initialize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public static async Task InitializeModules(
        this IServiceProvider serviceProvider,
        IEnumerable<IModule> modules,
        CancellationToken cancellationToken = default)
    {
        foreach (var module in modules)
        {
            Console.WriteLine($"Initializing module '{module.Name}'...");
            await module.Initialize(serviceProvider, cancellationToken);
        }
    }
}
