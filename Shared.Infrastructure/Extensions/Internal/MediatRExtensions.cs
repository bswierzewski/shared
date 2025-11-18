using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Behaviors;

namespace Shared.Infrastructure.Extensions.Internal;

/// <summary>
/// Internal extension methods for configuring MediatR with behaviors and handlers.
/// </summary>
internal static class MediatRExtensions
{
    /// <summary>
    /// Adds MediatR with all pipeline behaviors and registers handlers from all modules.
    /// Behaviors are added in the following order:
    /// 1. LoggingBehavior - logs request start, completion, and timing
    /// 2. UnhandledExceptionBehavior - catches and logs unhandled exceptions
    /// 3. AuthorizationBehavior - enforces security policies
    /// 4. ValidationBehavior - validates request data using FluentValidation
    /// 5. PerformanceBehavior - monitors and logs slow requests
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modules">The list of modules to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection AddMediatRWithBehaviors(
        this IServiceCollection services,
        IList<IModule> modules)
    {
        services.AddMediatR(cfg =>
        {
            // Register handlers from all module assemblies
            if (modules.Count > 0)
            {
                foreach (var module in modules)                
                    cfg.RegisterServicesFromAssembly(module.GetType().Assembly);                
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
