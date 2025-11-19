using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Modules;
using Shared.Users.Domain.Entities;
using Shared.Users.Infrastructure.Persistence;

namespace Shared.Users.Infrastructure.Services;

/// <summary>
/// Background service that synchronizes module-defined roles and permissions with the database.
/// Runs on application startup to ensure database schema is consistent with module definitions.
///
/// Synchronization logic:
/// 1. For each module's permission/role: Add to DB if not exists
/// 2. For DB permissions/roles marked as IsModule: Deactivate if no longer defined in modules
/// 3. Custom (non-module) permissions/roles are never modified
/// </summary>
public class RolePermissionSynchronizationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IModuleRegistry _moduleRegistry;
    private readonly ILogger<RolePermissionSynchronizationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionSynchronizationService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for creating scopes.</param>
    /// <param name="moduleRegistry">The module registry to get module permissions and roles.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public RolePermissionSynchronizationService(
        IServiceProvider serviceProvider,
        IModuleRegistry moduleRegistry,
        ILogger<RolePermissionSynchronizationService> logger)
    {
        _serviceProvider = serviceProvider;
        _moduleRegistry = moduleRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Starts the service, which synchronizes roles and permissions on application startup.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting role and permission synchronization...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

            // Synchronize permissions and roles from all modules
            await SynchronizePermissionsAsync(dbContext, cancellationToken);
            await SynchronizeRolesAsync(dbContext, cancellationToken);

            _logger.LogInformation("Role and permission synchronization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during role and permission synchronization");
            throw;
        }
    }

    /// <summary>
    /// Stops the service (no-op for this background service).
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Synchronizes permissions from all modules with the database.
    /// </summary>
    private async Task SynchronizePermissionsAsync(UsersDbContext dbContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Synchronizing permissions from modules...");

        // Collect all module-defined permissions
        var modulePermissions = _moduleRegistry.Modules
            .SelectMany(m => m.Permissions.Select(p => (ModuleName: m.Name, Permission: p)))
            .ToList();

        var dbPermissions = await dbContext.Permissions.ToListAsync(cancellationToken);

        // 1. Add missing module permissions
        foreach (var (moduleName, permission) in modulePermissions)
        {
            var existing = dbPermissions.FirstOrDefault(p => p.Name == permission.Name);
            if (existing == null)
            {
                var newPermission = Domain.Entities.Permission.Create(
                    permission.Name,
                    permission.Description,
                    isModule: true,
                    moduleName: moduleName);

                dbContext.Permissions.Add(newPermission);
                _logger.LogInformation("Added new module permission: {PermissionName} from module '{ModuleName}'",
                    permission.Name, moduleName);
            }
            else if (!existing.IsModule)
            {
                // If permission exists but wasn't marked as module permission, mark it now
                existing.IsModule = true;
                existing.ModuleName = moduleName;
                _logger.LogInformation("Marked permission as module permission: {PermissionName} from module '{ModuleName}'",
                    permission.Name, moduleName);
            }
        }

        // 2. Deactivate module permissions that are no longer defined in any module
        var activeModulePermissions = dbPermissions.Where(p => p.IsModule && p.IsActive).ToList();
        var modulePermissionNames = modulePermissions.Select(mp => mp.Permission.Name).ToHashSet();

        foreach (var permission in activeModulePermissions)
        {
            if (!modulePermissionNames.Contains(permission.Name))
            {
                permission.Deactivate();
                _logger.LogInformation("Deactivated removed module permission: {PermissionName}",
                    permission.Name);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Synchronizes roles from all modules with the database.
    /// </summary>
    private async Task SynchronizeRolesAsync(UsersDbContext dbContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Synchronizing roles from modules...");

        // Collect all module-defined roles
        var moduleRoles = _moduleRegistry.Modules
            .SelectMany(m => m.Roles.Select(r => (ModuleName: m.Name, Role: r)))
            .ToList();

        var dbRoles = await dbContext.Roles
            .Include(r => r.Permissions)
            .ToListAsync(cancellationToken);

        var dbPermissions = await dbContext.Permissions.ToListAsync(cancellationToken);

        // 1. Add missing module roles and assign their permissions
        foreach (var (moduleName, moduleRole) in moduleRoles)
        {
            var existing = dbRoles.FirstOrDefault(r => r.Name == moduleRole.Name);
            if (existing == null)
            {
                var newRole = Domain.Entities.Role.Create(
                    moduleRole.Name,
                    moduleRole.Description,
                    isModule: true,
                    moduleName: moduleName);

                // Add permissions to the role
                foreach (var modulePerm in moduleRole.Permissions)
                {
                    var dbPerm = dbPermissions.FirstOrDefault(p => p.Name == modulePerm.Name);
                    if (dbPerm != null && !newRole.Permissions.Contains(dbPerm))
                    {
                        newRole.AddPermission(dbPerm);
                    }
                }

                dbContext.Roles.Add(newRole);
                _logger.LogInformation("Added new module role: {RoleName} from module '{ModuleName}' with {PermissionCount} permissions",
                    moduleRole.Name, moduleName, moduleRole.Permissions.Count);
            }
            else
            {
                // Mark as module role if not already marked
                if (!existing.IsModule)
                {
                    existing.IsModule = true;
                    existing.ModuleName = moduleName;
                    _logger.LogInformation("Marked role as module role: {RoleName} from module '{ModuleName}'",
                        moduleRole.Name, moduleName);
                }

                // Update permissions for the role (sync with module definition)
                var modulePermNames = moduleRole.Permissions.Select(p => p.Name).ToHashSet();

                // Remove permissions that are no longer in the module definition
                var permsToRemove = existing.Permissions
                    .Where(p => !modulePermNames.Contains(p.Name))
                    .ToList();

                foreach (var perm in permsToRemove)
                {
                    existing.RemovePermission(perm);
                    _logger.LogDebug("Removed permission {PermissionName} from role {RoleName}",
                        perm.Name, existing.Name);
                }

                // Add permissions that are in the module definition but not in the role
                foreach (var modulePerm in moduleRole.Permissions)
                {
                    var dbPerm = dbPermissions.FirstOrDefault(p => p.Name == modulePerm.Name);
                    if (dbPerm != null && !existing.Permissions.Contains(dbPerm))
                    {
                        existing.AddPermission(dbPerm);
                        _logger.LogDebug("Added permission {PermissionName} to role {RoleName}",
                            modulePerm.Name, existing.Name);
                    }
                }
            }
        }

        // 2. Deactivate module roles that are no longer defined in any module
        var activeModuleRoles = dbRoles.Where(r => r.IsModule && r.IsActive).ToList();
        var moduleRoleNames = moduleRoles.Select(mr => mr.Role.Name).ToHashSet();

        foreach (var role in activeModuleRoles)
        {
            if (!moduleRoleNames.Contains(role.Name))
            {
                role.Deactivate();
                _logger.LogInformation("Deactivated removed module role: {RoleName}", role.Name);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
