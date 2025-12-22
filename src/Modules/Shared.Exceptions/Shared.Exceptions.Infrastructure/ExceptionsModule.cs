using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Authorization;
using Shared.Exceptions.Application;
using Shared.Exceptions.Infrastructure.Endpoints;
using Shared.Infrastructure.Modules;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Exceptions module - provides testing endpoints for error handling and exception scenarios.
/// </summary>
public class ExceptionsModule : IModule
{
    public string Name => Module.Name;

    public IEnumerable<Permission> Permissions { get; }

    public IEnumerable<Role> Roles { get; }

    public ExceptionsModule()
    {
        var builder = new AuthorizationBuilder(Module.Name);

        builder
            .AddPermission(Module.Permissions.View, "View Exceptions", "Allows viewing exception logs")
            .AddPermission(Module.Permissions.Create, "Create Exceptions", "Allows generating test exceptions")
            .AddPermission(Module.Permissions.Edit, "Edit Exceptions", "Allows modifying exception settings")
            .AddPermission(Module.Permissions.Delete, "Delete Exceptions", "Allows clearing exception logs");

        builder
            .AddRole(Module.Roles.Admin, "Exceptions Admin", "Full access to exception module", [
                Module.Permissions.View,
                Module.Permissions.Create,
                Module.Permissions.Edit,
                Module.Permissions.Delete,
            ])
            .AddRole(Module.Roles.Editor, "Exceptions Editor", "Can manage exceptions but not configuration", [
                Module.Permissions.View,
                Module.Permissions.Create,
                Module.Permissions.Edit
            ])
            .AddRole(Module.Roles.Viewer, "Exceptions Viewer", "Read-only access", [
                Module.Permissions.View
            ]);

        (Permissions, Roles) = builder.Build();
    }
    /// <summary>
    /// Registers Exceptions module services and MediatR handlers.
    /// This module is responsible for registering everything it needs.
    /// </summary>
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register module services using fluent ModuleBuilder API
        services.AddModule(configuration, Module.Name)
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

    /// <inheritdoc/>
    public Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
