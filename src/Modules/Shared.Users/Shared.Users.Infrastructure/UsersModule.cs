using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Authorization;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Persistence.Migrations;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.Options;
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
/// Automatic Registration:
/// This module uses IModuleAssembly markers (ApplicationAssembly, InfrastructureAssembly) for automatic discovery.
/// The following are registered automatically by the module infrastructure:
/// - MediatR handlers from Application and Infrastructure assemblies
/// - FluentValidation validators from Application and Infrastructure assemblies
/// - HTTP endpoints via IModuleEndpoints (UserEndpoints class)
///
/// Integration:
/// 1. Module is auto-discovered and loaded via AddModules()
/// 2. Call app.UseModules() in middleware pipeline (after authentication)
/// 3. Inject IUser to read authenticated user and their roles/permissions
/// </summary>
public class UsersModule : IModule
{
    /// <summary>
    /// Gets the unique name of the Users module
    /// </summary>
    public string Name => "users";

    /// <summary>
    /// Registers Users module services, DbContext, and module-specific configuration.
    ///
    /// Note: MediatR handlers, validators, and endpoints are registered automatically via IModuleAssembly markers.
    /// This method only registers module-specific services (DbContext, authentication, custom services).
    /// </summary>
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // ==================== AUTOMATIC REGISTRATION ====================
        // The following are automatically registered by the module infrastructure:
        // - MediatR handlers from Application and Infrastructure assemblies (ApplicationAssembly, InfrastructureAssembly markers)
        // - FluentValidation validators from Application and Infrastructure assemblies
        // - HTTP endpoints from UserEndpoints class (implements IModuleEndpoints)
        //
        // See: src/Shared.Infrastructure/Modules/ModuleExtensions.cs for automatic registration logic
        // ================================================================

        // Register HttpContextAccessor (required for IUser implementation to access ClaimsPrincipal)
        services.AddHttpContextAccessor();

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

        // ==================== AUTOMATIC AUTHENTICATION SETUP ====================
        // Register authentication options
        services.Configure<UserAuthenticationOptions>(configuration.GetSection(UserAuthenticationOptions.SectionName));

        // Automatically setup authentication based on configured provider
        var authOptions = new UserAuthenticationOptions();
        configuration.GetSection(UserAuthenticationOptions.SectionName).Bind(authOptions);

        if (authOptions.Provider != AuthenticationProvider.None)
        {
            // Setup authentication builder
            var authBuilder = services.AddAuthentication();

            // Configure based on provider
            switch (authOptions.Provider)
            {
                case AuthenticationProvider.Supabase:
                    // Configure Supabase JWT Bearer
                    services.Configure<SupabaseOptions>(configuration.GetSection(SupabaseOptions.SectionName));
                    authBuilder.AddSupabaseJwtBearer();
                    break;

                case AuthenticationProvider.Clerk:
                    // Configure Clerk JWT Bearer
                    services.Configure<ClerkOptions>(configuration.GetSection(ClerkOptions.SectionName));
                    authBuilder.AddClerkJwtBearer();
                    break;
            }
        }
    }

    /// <summary>
    /// Configures the Users module middleware pipeline.
    /// Adds JIT provisioning and claims enrichment middleware.
    ///
    /// Note: HTTP endpoints are mapped automatically by the module infrastructure via IModuleEndpoints.
    /// See UserEndpoints class for endpoint definitions.
    /// </summary>
    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        // Add middleware for JIT provisioning and claims enrichment
        // This middleware runs after authentication and enriches the ClaimsPrincipal with user data
        app.UseMiddleware<JITProvisioningMiddleware>();

        // HTTP endpoints are mapped automatically by ModuleExtensions.UseModules()
        // See: UserEndpoints class (implements IModuleEndpoints)
    }

    /// <summary>
    /// Initializes the Users module by running database migrations and synchronizing roles/permissions.
    /// This method is called automatically during application startup via InitializeApplicationAsync().
    /// </summary>
    public async Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await new MigrationService<UsersDbContext>(serviceProvider).MigrateAsync(cancellationToken);
        await new RolePermissionSynchronizationService(serviceProvider).InitializeAsync(cancellationToken);
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
