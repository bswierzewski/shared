using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Modules;
using Shared.Users.Application.Abstractions;

namespace Shared.Users.Infrastructure.Services;

/// <summary>
/// Synchronizes module-defined roles and permissions with the database.
/// Ensures database state matches the definitions in code.
///
/// Algorithm:
/// 1. Collect all permissions and roles from modules
/// 2. Sync permissions: Add missing, Activate/Deactivate based on module definitions
/// 3. Sync roles: Add missing, Activate/Deactivate, Update role permissions
/// </summary>
/// <remarks>
/// Initializes a new instance of the RolePermissionSynchronizationService.
/// </remarks>
/// <param name="serviceProvider">The service provider for dependency injection.</param>
public class AuthorizationSyncService(IServiceProvider serviceProvider)
{
    private readonly IEnumerable<IModule> _modules = serviceProvider.GetServices<IModule>();
    private readonly ILogger<AuthorizationSyncService> _logger = serviceProvider.GetRequiredService<ILogger<AuthorizationSyncService>>();

    /// <summary>
    /// Asynchronously initializes the role and permission synchronization process.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IUsersDbContext>();

        // Step 1: Sync permissions first (roles depend on permissions)
        await SyncPermissions(context, ct);

        // Step 2: Sync roles with their permissions
        await SyncRoles(context, ct);

        _logger.LogInformation("Roles and permissions synchronized successfully");
    }

    /// <summary>
    /// Synchronizes permissions from modules with the database.
    /// - Adds missing permissions
    /// - Activates permissions that are in module definitions
    /// - Deactivates permissions that are no longer in module definitions
    /// </summary>
    private async Task SyncPermissions(IUsersDbContext context, CancellationToken ct)
    {
        // Collect all permissions from all modules
        var modulePermissions = _modules
            .SelectMany(m => m.Permissions)
            .ToList();

        // Get existing module permissions from database
        var databasePermissions = await context.Permissions
            .Where(p => p.IsModule)
            .ToListAsync(ct);

        var databasePermissionsMap = databasePermissions.ToDictionary(p => p.Name);
        var modulePermissionNames = modulePermissions.Select(p => p.Name).ToHashSet();

        // Process permissions from modules
        foreach (var permissionDef in modulePermissions)
        {
            if (databasePermissionsMap.TryGetValue(permissionDef.Name, out var existingPermission))
            {
                // Permission exists in DB - activate if needed
                if (!existingPermission.IsActive)
                {
                    existingPermission.Activate();
                    _logger.LogInformation("Activated permission: {PermissionName}", permissionDef.Name);
                }
                // Could also update DisplayName/Description here if needed
            }
            else
            {
                // Permission doesn't exist in DB - create it
                var newPermission = Domain.Entities.Permission.Create(
                    permissionDef.Name,
                    permissionDef.Description,
                    isModule: true,
                    moduleName: permissionDef.Module);

                context.Permissions.Add(newPermission);
                databasePermissionsMap.Add(newPermission.Name, newPermission);
                _logger.LogInformation("Added new permission: {PermissionName}", permissionDef.Name);
            }
        }

        // Deactivate permissions that are no longer in module definitions
        foreach (var dbPermission in databasePermissions.Where(p => p.IsActive && !modulePermissionNames.Contains(p.Name)))
        {
            dbPermission.Deactivate();
            _logger.LogInformation("Deactivated permission: {PermissionName}", dbPermission.Name);
        }

        await context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Synchronizes roles from modules with the database.
    /// - Adds missing roles with their permissions
    /// - Activates roles that are in module definitions
    /// - Updates role permissions to match module definitions
    /// - Deactivates roles that are no longer in module definitions
    /// </summary>
    private async Task SyncRoles(IUsersDbContext context, CancellationToken ct)
    {
        // Collect all roles from all modules
        var moduleRoles = _modules
            .SelectMany(m => m.Roles)
            .ToList();

        // Get existing module roles from database (with permissions loaded)
        var databaseRoles = await context.Roles
            .Include(r => r.Permissions)
            .Where(r => r.IsModule)
            .ToListAsync(ct);

        // Get all permissions for mapping
        var databasePermissions = await context.Permissions.ToListAsync(ct);
        var databasePermissionsMap = databasePermissions.ToDictionary(p => p.Name);

        var databaseRolesMap = databaseRoles.ToDictionary(r => r.Name);
        var moduleRoleNames = moduleRoles.Select(r => r.Name).ToHashSet();

        // Process roles from modules
        foreach (var roleDef in moduleRoles)
        {
            // Get domain permissions for this role
            var domainPermissions = roleDef.Permissions
                .Select(p => databasePermissionsMap[p.Name])
                .ToList();

            if (databaseRolesMap.TryGetValue(roleDef.Name, out var existingRole))
            {
                // Role exists in DB - activate if needed
                if (!existingRole.IsActive)
                {
                    existingRole.Activate();
                    _logger.LogInformation("Activated role: {RoleName}", roleDef.Name);
                }

                // Reset permissions to match module definition
                // Remove all current permissions
                foreach (var permission in existingRole.Permissions.ToList())
                    existingRole.RemovePermission(permission);

                // Add all permissions from module definition
                foreach (var permission in domainPermissions)
                    existingRole.AddPermission(permission);

                _logger.LogInformation("Updated permissions for role {RoleName}: {PermissionCount} permissions",
                    roleDef.Name, domainPermissions.Count);
            }
            else
            {
                // Role doesn't exist in DB - create it
                var newRole = Domain.Entities.Role.Create(
                    roleDef.Name,
                    roleDef.DisplayName,
                    isModule: true,
                    moduleName: roleDef.Module,
                    permissions: domainPermissions.AsReadOnly());

                context.Roles.Add(newRole);
                databaseRolesMap.Add(newRole.Name, newRole);
                _logger.LogInformation("Added new role: {RoleName} with {PermissionCount} permissions",
                    roleDef.Name, domainPermissions.Count);
            }
        }

        // Deactivate roles that are no longer in module definitions
        foreach (var dbRole in databaseRoles.Where(r => r.IsActive && !moduleRoleNames.Contains(r.Name)))
        {
            dbRole.Deactivate();
            _logger.LogInformation("Deactivated role: {RoleName}", dbRole.Name);
        }

        await context.SaveChangesAsync(ct);
    }
}
