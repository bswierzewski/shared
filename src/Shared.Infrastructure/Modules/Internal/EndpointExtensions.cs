using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Modules.Internal;

/// <summary>
/// Internal extension methods for automatic endpoint discovery and registration.
/// </summary>
internal static class EndpointExtensions
{
    /// <summary>
    /// Scans all module assemblies (marked with IModuleAssembly) and registers
    /// all IModuleEndpoints implementations in the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection AddModuleEndpoints(
        this IServiceCollection services)
    {
        var moduleAssemblies = AssemblyScanner.GetModuleAssemblies();

        foreach (var assembly in moduleAssemblies)
        {
            var endpointTypes = assembly.GetTypes()
                .Where(t => typeof(IModuleEndpoints).IsAssignableFrom(t)
                         && !t.IsInterface
                         && !t.IsAbstract
                         && t.IsClass);

            foreach (var endpointType in endpointTypes)
            {
                // Register as scoped service so it can use DI
                services.AddScoped(typeof(IModuleEndpoints), endpointType);
            }
        }

        return services;
    }

    /// <summary>
    /// Automatically invokes MapEndpoints() on all registered IModuleEndpoints implementations.
    /// This should be called in the middleware pipeline after UseRouting() but before UseEndpoints().
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    internal static IApplicationBuilder MapModuleEndpoints(
        this IApplicationBuilder app)
    {
        if (app is not IEndpointRouteBuilder endpoints)
            return app;

        // Create a scope to resolve scoped services
        using var scope = app.ApplicationServices.CreateScope();
        var moduleEndpoints = scope.ServiceProvider.GetServices<IModuleEndpoints>();

        foreach (var endpoint in moduleEndpoints)
        {
            endpoint.MapEndpoints(endpoints);
        }

        return app;
    }
}
