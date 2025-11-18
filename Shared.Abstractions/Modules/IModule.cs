using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Security;

namespace Shared.Abstractions.Modules;

/// <summary>
/// Defines a module within the application.
/// Each module should implement this interface to register its services and configure its pipeline.
/// Modules can be dynamically enabled or disabled via configuration.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the unique name of the module (lowercase, e.g., "users", "orders").
    /// This name is used in configuration to enable/disable the module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the permissions defined by this module.
    /// Override this method to provide module-specific permissions.
    /// </summary>
    /// <returns>A collection of permissions provided by this module.</returns>
    IEnumerable<Permission> GetPermissions() => Enumerable.Empty<Permission>();

    /// <summary>
    /// Gets the roles defined by this module.
    /// Override this method to provide module-specific roles with their associated permissions.
    /// </summary>
    /// <returns>A collection of roles provided by this module.</returns>
    IEnumerable<Role> GetRoles() => Enumerable.Empty<Role>();

    /// <summary>
    /// Registers the module's services with the dependency injection container.
    /// Called during application startup for all enabled modules.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void Register(IServiceCollection services);

    /// <summary>
    /// Configures the module's middleware and application pipeline.
    /// Called after all modules have been registered.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    void Use(IApplicationBuilder app);
}
