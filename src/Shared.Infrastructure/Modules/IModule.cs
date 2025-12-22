using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Authorization;

namespace Shared.Infrastructure.Modules;

/// <summary>
/// Defines a module's infrastructure configuration including service registration,
/// middleware configuration, and initialization logic.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Name for the module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Permissions defined by the module.
    /// </summary>
    IEnumerable<Permission> Permissions { get; }

    /// <summary>
    /// Roles defined by the module.
    /// </summary>
    IEnumerable<Role> Roles { get; }

    /// <summary>
    /// Registers the module's services with the dependency injection container.
    /// Called during application startup for all enabled modules.
    /// </summary>
    void Register(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Configures the module's middleware and application pipeline.
    /// Called after all modules have been registered.
    /// </summary>
    void Use(IApplicationBuilder app, IConfiguration configuration);

    /// <summary>
    /// Performs initialization tasks for the module before the application starts handling requests.
    /// This is called after all modules have been registered and configured.
    /// Use this for tasks like running migrations, seeding data, or synchronizing configuration with the database.
    /// </summary>
    Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        => Task.CompletedTask; // Default: no initialization needed
}
