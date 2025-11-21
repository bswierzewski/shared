using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Authorization;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Modules;
using Shared.Infrastructure.Persistence.Migrations;
using Shared.Users.Application;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.Options;
using Shared.Users.Infrastructure.Endpoints;
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
/// Integration:
/// 1. Module is auto-discovered and loaded in AddModules()
/// 2. Call app.UseUsersModule() in middleware pipeline (after authentication)
/// 3. Inject IUser to read authenticated user and their roles/permissions
/// </summary>
public class UsersModule : IModule
{
    /// <summary>
    /// Gets the unique name of the Users module
    /// </summary>
    public string Name => "users";

    /// <summary>
    /// Register Users module services, DbContext, and command/query handlers
    /// </summary>
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register database options from configuration
        // Binds "Users:Database" section from appsettings.json to DbContextOptions
        services.Configure<Options.DbContextOptions>(configuration.GetSection(Options.DbContextOptions.SectionName));

        // Register Supabase authentication options
        // Used by SupabaseJwtBearerExtensions for JWT validation
        services.Configure<SupabaseOptions>(configuration.GetSection(SupabaseOptions.SectionName));

        // Register DbContext with PostgreSQL using configured options
        // Options are injected through IOptions<DbContextOptions>
        services.AddDbContext<UsersDbContext>((sp, dbOptions) =>
        {
            // Get the configured database options from DI
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Options.DbContextOptions>>().Value;

            // Configure PostgreSQL with connection string
            dbOptions.UseNpgsql(options.ConnectionString)
                     .AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor>());

            // Configure additional EF Core options if needed
            if (options.EnableDetailedErrors)
                dbOptions.EnableDetailedErrors();

            if (options.EnableSensitiveDataLogging)
                dbOptions.EnableSensitiveDataLogging();
        });

        // Register DbContext abstractions
        services.AddScoped<IUsersReadDbContext>(sp => sp.GetRequiredService<UsersDbContext>());
        services.AddScoped<IUsersWriteDbContext>(sp => sp.GetRequiredService<UsersDbContext>());

        // Register provisioning service
        services.AddScoped<IUserProvisioningService, UserProvisioningService>();

        // Register HttpContextAccessor (required for IUser implementation to access ClaimsPrincipal)
        services.AddHttpContextAccessor();

        // Register IUser implementation (reads from enriched ClaimsPrincipal)
        services.AddScoped<IUser, CurrentUserService>();

        // Register handlers (MediatR) - scan Application and Infrastructure assemblies
        // Uses AssemblyMarker classes to identify the correct assemblies
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            config.RegisterServicesFromAssembly(typeof(InfrastructureAssemblyMarker).Assembly);
        });

    }

    /// <summary>
    /// Configure middleware pipeline - adds JIT provisioning and claims enrichment
    /// Also maps HTTP endpoints for user management operations
    /// </summary>
    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        // Add middleware for JIT provisioning and claims enrichment
        app.UseMiddleware<JITProvisioningMiddleware>();

        // Map user management endpoints
        // Note: IApplicationBuilder is typically a WebApplication which also implements IEndpointRouteBuilder
        // This allows us to map endpoints directly without requiring a separate parameter
        if (app is Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapUserEndpoints();
        }
    }

    /// <summary>
    /// Initializes the Users module by running migrations and synchronizing roles/permissions.
    /// </summary>
    public async Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<UsersModule>>();

        try
        {
            logger.LogInformation("Starting UsersModule initialization...");

            // Run migrations
            logger.LogInformation("Running database migrations for UsersModule...");
            var migrationService = new MigrationService<UsersDbContext>(serviceProvider,
                scope.ServiceProvider.GetRequiredService<ILogger<MigrationService<UsersDbContext>>>());
            await migrationService.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrations completed successfully");

            // Synchronize roles and permissions
            logger.LogInformation("Synchronizing roles and permissions...");
            var syncService = new RolePermissionSynchronizationService(
                serviceProvider,
                scope.ServiceProvider.GetRequiredService<IReadOnlyCollection<IModule>>(),
                scope.ServiceProvider.GetRequiredService<ILogger<RolePermissionSynchronizationService>>());
            await syncService.InitializeAsync(cancellationToken);
            logger.LogInformation("UsersModule initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during UsersModule initialization");
            throw;
        }
    }

    /// <summary>
    /// Define permissions available in this module
    /// </summary>
    public IEnumerable<Permission> GetPermissions()
    {
        return new[]
        {
            new Permission("users.view", "View users", Name, "View user information"),
            new Permission("users.create", "Create users", Name, "Create new users"),
            new Permission("users.edit", "Edit users", Name, "Edit user profiles"),
            new Permission("users.delete", "Delete users", Name, "Delete users"),
            new Permission("users.assign_roles", "Assign roles", Name, "Assign roles to users"),
            new Permission("users.manage_permissions", "Manage permissions", Name, "Grant/revoke permissions"),
        };
    }

    /// <summary>
    /// Define roles available in this module
    /// </summary>
    public IEnumerable<Role> GetRoles()
    {
        var permissions = GetPermissions().ToList();

        return new[]
        {
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
        };
    }
}
