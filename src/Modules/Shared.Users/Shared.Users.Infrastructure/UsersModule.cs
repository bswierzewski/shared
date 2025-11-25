using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Authorization;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Persistence.Migrations;
using Shared.Users.Application;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.Options;
using Shared.Users.Infrastructure.Endpoints;
using Shared.Users.Infrastructure.Extensions.JwtBearers;
using Shared.Users.Infrastructure.Middleware;
using Shared.Users.Infrastructure.Persistence;
using Shared.Users.Infrastructure.Services;

namespace Shared.Users.Infrastructure;

/// <summary>
/// Users module - provides JIT user provisioning with role-based access control.
///
/// Features:
/// - Just-In-Time (JIT) user provisioning from external identity providers
/// - Email-based provider linking (multiple external providers → single internal user)
/// - Role and permission management
/// - Claims enrichment via JitProvisioningMiddleware
/// - IUser implementation reading from enriched ClaimsPrincipal
/// - Atomic caching with cache stampede prevention
///
/// This module is responsible for:
/// - Registering its own services, MediatR handlers, and validators in Register()
/// - Configuring its own middleware and endpoints in Use()
/// </summary>
public class UsersModule : IModule
{
    /// <summary>
    /// Gets the unique name of the Users module
    /// </summary>
    public string Name => "users";

    /// <summary>
    /// Registers Users module services, DbContext, MediatR handlers, and validators.
    /// This module is responsible for registering everything it needs.
    /// </summary>
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register HttpContextAccessor (required for IUser implementation to access ClaimsPrincipal)
        services.AddHttpContextAccessor();

        // Register DbContext
        services.AddDbContext<UsersDbContext>((sp, dbOptions) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("Users")
                ?? throw new InvalidOperationException("Connection string 'Users' not found in configuration");

            dbOptions.UseNpgsql(connectionString)
                     .AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor>());
        });

        // Register DbContext abstractions
        services.AddScoped<IUsersReadDbContext>(sp => sp.GetRequiredService<UsersDbContext>());
        services.AddScoped<IUsersWriteDbContext>(sp => sp.GetRequiredService<UsersDbContext>());

        // Register provisioning service
        services.AddScoped<IUserProvisioningService, UserProvisioningService>();

        // Register IUser implementation (reads from enriched ClaimsPrincipal)
        services.AddScoped<IUser, CurrentUserService>();

        // Register MediatR handlers from Application and Infrastructure assemblies
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationAssembly).Assembly);
            config.RegisterServicesFromAssembly(typeof(InfrastructureAssembly).Assembly);
        });

        // Register FluentValidation validators from Application and Infrastructure assemblies
        services.AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly);
        services.AddValidatorsFromAssembly(typeof(InfrastructureAssembly).Assembly);

        // Configure authentication options
        services.Configure<UserAuthenticationOptions>(configuration.GetSection(UserAuthenticationOptions.SectionName));

        // Setup authentication based on configured provider
        var authOptions = new UserAuthenticationOptions();
        configuration.GetSection(UserAuthenticationOptions.SectionName).Bind(authOptions);

        if (authOptions.Provider != AuthenticationProvider.None)
        {
            var authBuilder = services.AddAuthentication();

            switch (authOptions.Provider)
            {
                case AuthenticationProvider.Supabase:
                    services.Configure<SupabaseOptions>(configuration.GetSection(SupabaseOptions.SectionName));
                    authBuilder.AddSupabaseJwtBearer();
                    break;

                case AuthenticationProvider.Clerk:
                    services.Configure<ClerkOptions>(configuration.GetSection(ClerkOptions.SectionName));
                    authBuilder.AddClerkJwtBearer();
                    break;
            }
        }
    }

    /// <summary>
    /// Configures the Users module middleware pipeline.
    /// Adds JIT provisioning and claims enrichment middleware, and maps endpoints.
    /// </summary>
    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        // Add middleware for JIT provisioning and claims enrichment
        app.UseMiddleware<JITProvisioningMiddleware>();

        // Map user management endpoints
        if (app is Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapUserEndpoints();
        }
    }

    /// <summary>
    /// Defines the permissions available in the Users module.
    /// These permissions are automatically synchronized to the database during module initialization.
    /// </summary>
    public IEnumerable<Permission> GetPermissions()
    {
        return
        [
            new Permission("users.view", "View users", Name, "View user information"),
            new Permission("users.create", "Create users", Name, "Create new users"),
            new Permission("users.edit", "Edit users", Name, "Edit user profiles"),
            new Permission("users.delete", "Delete users", Name, "Delete users"),
            new Permission("users.assign_roles", "Assign roles", Name, "Assign roles to users"),
            new Permission("users.manage_permissions", "Manage permissions", Name, "Grant/revoke permissions"),
        ];
    }

    /// <summary>
    /// Defines the roles available in the Users module.
    /// These roles are automatically synchronized to the database during module initialization.
    /// </summary>
    public IEnumerable<Role> GetRoles()
    {
        var permissions = GetPermissions().ToList();

        return
        [
            new Role(
                "admin",
                "Administrator",
                Name,
                permissions.AsReadOnly()),

            new Role(
                "editor",
                "Editor",
                Name,
                permissions.Where(p => p.Name is "users.view" or "users.edit" or "users.assign_roles").ToList().AsReadOnly()),

            new Role(
                "viewer",
                "Viewer",
                Name,
                permissions.Where(p => p.Name is "users.view").ToList().AsReadOnly())
        ];
    }
}
