using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Abstractions;
using Shared.Abstractions.Authorization;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Modules;
using Shared.Infrastructure.Persistence.Migrations;
using Shared.Users.Application;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.Options;
using Shared.Users.Domain;
using Shared.Users.Infrastructure.ClaimsTransformations;
using Shared.Users.Infrastructure.Endpoints;
using Shared.Users.Infrastructure.Extensions.Supabase;
using Shared.Users.Infrastructure.Persistence;
using Shared.Users.Infrastructure.Services;

namespace Shared.Users.Infrastructure;

/// <summary>
/// Users module - provides JIT user provisioning with role-based access control.
/// </summary>
public partial class UsersModule : IModule
{
    public string Name => Module.Name;

    public IEnumerable<Permission> Permissions { get; }

    public IEnumerable<Role> Roles { get; }

    public UsersModule()
    {
        var builder = new AuthorizationBuilder(Module.Name);

        builder
            .AddPermission(Module.Permissions.View, "View Users", "Allows viewing user profiles and list")
            .AddPermission(Module.Permissions.Create, "Create Users", "Allows registering new users")
            .AddPermission(Module.Permissions.Edit, "Edit Users", "Allows modifying existing user accounts")
            .AddPermission(Module.Permissions.Delete, "Delete Users", "Allows removing users from the system");

        builder
            .AddRole(Module.Roles.Admin, "Administrator", "Full management access", [
                Module.Permissions.View,
                Module.Permissions.Create,
                Module.Permissions.Edit,
                Module.Permissions.Delete,
            ])
            .AddRole(Module.Roles.Editor, "Editor", "Can view and manage users but not delete them", [
                Module.Permissions.View,
                Module.Permissions.Edit,
                Module.Permissions.Create
            ])
            .AddRole(Module.Roles.Viewer, "Viewer", "Read-only access", [
                Module.Permissions.View
            ]);

        (Permissions, Roles) = builder.Build();
    }

    /// <inheritdoc/>
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        // Register module services using fluent ModuleBuilder API
        services.AddModule(configuration, Module.Name)
            .AddOptions((svc, config) =>
            {
                svc.ConfigureOptions<UsersDbContextOptions>(config);
                svc.ConfigureOptions<ClerkOptions>(config);
                svc.ConfigureOptions<SupabaseOptions>(config);
            })
            .AddPostgres<UsersDbContext, IUsersDbContext>(sp => sp.GetRequiredService<IOptions<UsersDbContextOptions>>().Value.ConnectionString)
            .AddCQRS(typeof(ApplicationAssembly).Assembly, typeof(InfrastructureAssembly).Assembly)
            .Build();

        // Register service
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<IClaimsTransformation, AuthorizationClaimsTransformation>();

        // Register IUser implementation (reads from enriched ClaimsPrincipal)
        services.AddScoped<IUser, CurrentUserService>();
    }

    /// <summary>
    /// Configures the Users module middleware pipeline.
    /// Adds user provisioning and claims enrichment middleware, and maps endpoints.
    /// </summary>
    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        // Map user management endpoints
        if (app is Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpointRouteBuilder)
            endpointRouteBuilder.MapUserEndpoints();
    }

    /// <summary>
    /// Initializes the Users module by running database migrations.
    /// </summary>
    public async Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await new MigrationService<UsersDbContext>(serviceProvider).MigrateAsync(cancellationToken);
        await new AuthorizationSyncService(serviceProvider).InitializeAsync(cancellationToken);
    }
}
