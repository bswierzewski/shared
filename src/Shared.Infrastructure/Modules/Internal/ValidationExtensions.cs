using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Modules.Internal;

/// <summary>
/// Internal extension methods for configuring FluentValidation.
/// </summary>
internal static class ValidationExtensions
{
    /// <summary>
    /// Registers FluentValidation validators from all module assemblies.
    /// Automatically scans assemblies marked with IModuleAssembly for validators.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection AddValidatorsFromModules(
        this IServiceCollection services)
    {
        var moduleAssemblies = AssemblyScanner.GetModuleAssemblies();

        foreach (var assembly in moduleAssemblies)
            services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
