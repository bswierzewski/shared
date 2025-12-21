using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Authorization;
using Shared.Abstractions.Modules;
using Shared.Exceptions.Application;
using Shared.Exceptions.Infrastructure.Endpoints;
using Shared.Infrastructure.Modules;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Exceptions module - provides testing endpoints for error handling and exception scenarios.
///
/// Features:
/// - Unhandled error testing
/// - Success response testing
/// - Error response testing
/// - Role-based access control testing
///
/// This module is responsible for:
/// - Registering its own services, MediatR handlers, and validators in Register()
/// - Configuring its own middleware and endpoints in Use()
/// </summary>
public class ExceptionsModule : IModule
{
    /// <summary>
    /// Gets the unique name of the Exceptions module
    /// </summary>
    public string Name => "Exceptions";

    /// <summary>
    /// Registers Exceptions module services and MediatR handlers.
    /// This module is responsible for registering everything it needs.
    /// </summary>
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register module services using fluent ModuleBuilder API
        services.AddModule(configuration, Name)
            .AddCQRS(typeof(ApplicationAssembly).Assembly, typeof(InfrastructureAssembly).Assembly)
            .Build();
    }

    /// <summary>
    /// Configures the Exceptions module middleware pipeline.
    /// Maps exception testing endpoints.
    /// </summary>
    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        // Map exception testing endpoints
        if (app is Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapExceptionEndpoints();
        }
    }

    /// <summary>
    /// Initializes the Exceptions module.
    /// No database initialization needed for this module.
    /// </summary>
    public Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Defines the roles available in the Exceptions module.
    /// Each role contains its associated permissions.
    /// </summary>
    public IEnumerable<Role> GetRoles()
    {
        // Define permissions
        var testExceptions = new Permission(
            ModuleConstants.Permissions.Test,
            "Test Exceptions",
            Name,
            "Test exception handling");

        var testAdminExceptions = new Permission(
            ModuleConstants.Permissions.TestAdmin,
            "Test Admin Exceptions",
            Name,
            "Test admin-level exception handling");

        // Define roles
        return
        [
            new Role(
                ModuleConstants.Roles.Tester,
                "Exception Tester",
                Name,
                new[] { testExceptions, testAdminExceptions }.AsReadOnly(),
                "Role for testing exception handling")
        ];
    }
}
