using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Extensions.Internal;

/// <summary>
/// Internal extension methods for configuring FluentValidation.
/// </summary>
internal static class ValidationExtensions
{
    /// <summary>
    /// Registers FluentValidation validators from all module assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modules">The list of modules to scan for validators.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection AddValidatorsFromModules(
        this IServiceCollection services,
        IList<IModule> modules)
    {
        foreach (var module in modules)
            services.AddValidatorsFromAssembly(module.GetType().Assembly);

        return services;
    }
}
