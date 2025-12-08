using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Authorization;
using Shared.Abstractions.Modules;
using Shared.Exceptions.Infrastructure.Endpoints;
using Shared.Infrastructure.Modules;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Exception testing module - provides endpoints for testing exception handling and ProblemDetails responses.
///
/// Features:
/// - Test endpoints for all exception types (Validation, NotFound, Unauthorized, Forbidden, ServerError)
/// - Commands executed via MediatR for clean architecture
/// - Can be disabled via appsettings (Modules:Exceptions:Enabled = false)
///
/// Integration:
/// 1. Module is auto-discovered and loaded in AddModules()
/// 2. Can be disabled in appsettings.json
/// 3. Endpoints are mapped automatically if enabled
/// </summary>
public class ExceptionModule : IModule
{
    /// <summary>
    /// Gets the unique name of the Exception testing module
    /// </summary>
    public string Name => "exceptions";

    /// <summary>
    /// Register Exception module services and command handlers
    /// </summary>
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register module services using fluent ModuleBuilder API
        services.AddModule(configuration, Name)
            .AddCQRS(typeof(Application.ApplicationAssembly).Assembly, typeof(InfrastructureAssembly).Assembly)
            .Build();
    }

    /// <summary>
    /// Configure middleware pipeline and map exception test endpoints
    /// </summary>
    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        var endpoints = (IEndpointRouteBuilder)app;

        // Map exception test endpoints
        endpoints.MapExceptionEndpoints();
    }

    /// <summary>
    /// Initialize the Exception module (no-op for this module)
    /// </summary>
    public Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// No custom permissions for exception testing module
    /// </summary>
    public IEnumerable<Permission> GetPermissions()
    {
        return [];
    }

    /// <summary>
    /// No custom roles for exception testing module
    /// </summary>
    public IEnumerable<Role> GetRoles()
    {
        return [];
    }
}
