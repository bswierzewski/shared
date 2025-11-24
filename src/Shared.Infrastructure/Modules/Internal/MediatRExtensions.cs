using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Behaviors;

namespace Shared.Infrastructure.Modules.Internal;

/// <summary>
/// Internal extension methods for configuring MediatR with behaviors and handlers.
/// </summary>
internal static class MediatRExtensions
{
    /// <summary>
    /// Adds MediatR with all pipeline behaviors and registers handlers from all module assemblies.
    /// Automatically scans assemblies marked with IModuleAssembly for MediatR handlers.
    ///
    /// Behaviors are added in the following order:
    /// 1. LoggingBehavior - logs request start, completion, and timing
    /// 2. UnhandledExceptionBehavior - catches and logs unhandled exceptions
    /// 3. AuthorizationBehavior - enforces security policies
    /// 4. ValidationBehavior - validates request data using FluentValidation
    /// 5. PerformanceBehavior - monitors and logs slow requests
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection AddMediatRWithBehaviors(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            // Register handlers from all assemblies marked with IModuleAssembly
            var moduleAssemblies = AssemblyScanner.GetModuleAssemblies();

            if (moduleAssemblies.Count > 0)
            {
                foreach (var assembly in moduleAssemblies)
                    cfg.RegisterServicesFromAssembly(assembly);
            }
            else
            {
                // MediatR requires at least one assembly, register Infrastructure as fallback
                cfg.RegisterServicesFromAssembly(typeof(MediatRExtensions).Assembly);
            }

            // Add behaviors in execution order
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        return services;
    }
}
