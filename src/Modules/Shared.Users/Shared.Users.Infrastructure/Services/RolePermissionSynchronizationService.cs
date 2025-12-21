using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Modules;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain;

namespace Shared.Users.Infrastructure.Services;

/// <summary>
/// Synchronizes module-defined roles with the database.
/// Ensures database state matches the role definitions defined in code.
/// Permissions are automatically synchronized through the role-permission relationship.
/// </summary>
public class RolePermissionSynchronizationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IModule> _modules;
    private readonly ILogger<RolePermissionSynchronizationService> _logger;

    /// <summary>
    /// Initializes a new instance of the RolePermissionSynchronizationService.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public RolePermissionSynchronizationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _modules = serviceProvider.GetServices<IModule>();
        _logger = serviceProvider.GetRequiredService<ILogger<RolePermissionSynchronizationService>>();
    }

    /// <summary>
    /// Asynchronously initializes the role synchronization process.
    /// Permissions are automatically synchronized through EF Core's many-to-many relationship.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IUsersDbContext>();

        // Synchronize roles (permissions are embedded in roles and will be synced automatically)
        await SyncRoles(context, ct);

        _logger.LogInformation("Roles and permissions synchronized");
    }

    /// <summary>
    /// Synchronizes roles from modules with the database.
    /// Permissions are automatically created and associated through EF Core's M2M relationship.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SyncRoles(IUsersDbContext context, CancellationToken ct)
    {
        // Collect role definitions from all modules (abstraction types)
        var moduleRoles = _modules
            .SelectMany(m => m.GetRoles())
            .ToList();

        // Get existing roles and permissions from database (domain entities)
        var databaseRoles = await context.Roles
            .Include(r => r.Permissions)
            .ToListAsync(ct);

        var databaseRolesMap = databaseRoles.ToDictionary(r => r.Name);

        var databasePermissions = await context.Permissions.ToListAsync(ct);
        var databasePermissionsMap = databasePermissions.ToDictionary(p => p.Name);

        // Upsert roles - convert abstraction types to domain entities
        foreach (var roleDefinition in moduleRoles)
        {
            if (databaseRolesMap.TryGetValue(roleDefinition.Name, out var existingRole))
            {
                // Role exists - reactivate if needed
                if (!existingRole.IsActive)
                {
                    existingRole.Activate();
                }

                // TODO: Sync permissions for existing role (add new, remove deleted)
            }
            else
            {
                // New role - convert abstraction type to domain entity
                // First, ensure all permissions exist in database
                var domainPermissions = new List<Domain.Entities.Permission>();

                foreach (var permDefinition in roleDefinition.Permissions)
                {
                    if (databasePermissionsMap.TryGetValue(permDefinition.Name, out var existingPerm))
                    {
                        domainPermissions.Add(existingPerm);
                    }
                    else
                    {
                        // Create new permission domain entity
                        var newPermission = Domain.Entities.Permission.Create(
                            permDefinition.Name,
                            permDefinition.Description,
                            isModule: true,
                            moduleName: permDefinition.Module);

                        context.Permissions.Add(newPermission);
                        databasePermissionsMap.Add(newPermission.Name, newPermission);
                        domainPermissions.Add(newPermission);
                    }
                }

                // Create new role domain entity with permissions
                var newRole = Domain.Entities.Role.Create(
                    roleDefinition.Name,
                    roleDefinition.DisplayName,
                    isModule: true,
                    moduleName: roleDefinition.Module,
                    permissions: domainPermissions.AsReadOnly());

                context.Roles.Add(newRole);
                databaseRolesMap.Add(newRole.Name, newRole);
            }
        }

        // Deactivate roles that are no longer defined in modules
        var definitionNames = moduleRoles.Select(r => r.Name).ToHashSet();
        foreach (var role in databaseRoles.Where(r => r.IsModule && r.IsActive && !definitionNames.Contains(r.Name)))
        {
            role.Deactivate();
        }

        await context.SaveChangesAsync(ct);
    }
}
