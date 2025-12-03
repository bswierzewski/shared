using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Modules;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Services;

/// <summary>
/// Synchronizes module-defined roles and permissions with the database.
/// Ensures database state matches the module definitions defined in code.
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
    /// Asynchronously initializes the role and permission synchronization process.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var writeDbContext = scope.ServiceProvider.GetRequiredService<IUsersWriteDbContext>();

        // Synchronize permissions first (roles depend on them)
        var databasePermissions = await SyncPermissions(writeDbContext, ct);

        // Synchronize roles with the current permissions
        await SyncRoles(writeDbContext, databasePermissions, ct);

        _logger.LogInformation("Permissions and roles synchronized");
    }

    /// <summary>
    /// Synchronizes permissions from modules with the database.
    /// </summary>
    /// <param name="writeDbContext">The write database context.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A list of all permissions from the database.</returns>
    private async Task<List<Permission>> SyncPermissions(IUsersWriteDbContext writeDbContext, CancellationToken ct)
    {
        // Collect module definitions
        var definitions = _modules
            .SelectMany(m => m.GetPermissions().Select(p => (ModuleName: m.Name, Definition: p)))
            .ToList();

        // Get database state
        var databasePermissions = await writeDbContext.Permissions.ToListAsync(ct);
        var databasePermissionsMap = databasePermissions.ToDictionary(p => p.Name);

        // Upsert permissions
        foreach (var (moduleName, definition) in definitions)
        {
            if (databasePermissionsMap.TryGetValue(definition.Name, out var existing))
            {
                if (!existing.IsModule)
                    existing.MarkAsModulePermission(moduleName, definition.Description);
            }
            else
            {
                var newPermission = Permission.Create(definition.Name, definition.Description, isModule: true, moduleName: moduleName);
                writeDbContext.Permissions.Add(newPermission);
                databasePermissions.Add(newPermission);
            }
        }

        // Deactivate removed permissions
        var definitionNames = definitions.Select(x => x.Definition.Name).ToHashSet();
        foreach (var permission in databasePermissions.Where(p => p.IsModule && p.IsActive && !definitionNames.Contains(p.Name)))
            permission.Deactivate();

        await writeDbContext.SaveChangesAsync(ct);
        return databasePermissions;
    }

    /// <summary>
    /// Synchronizes roles from modules with the database.
    /// </summary>
    /// <param name="writeDbContext">The write database context.</param>
    /// <param name="allDatabasePermissions">List of all permissions currently in the database.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SyncRoles(IUsersWriteDbContext writeDbContext, List<Permission> allDatabasePermissions, CancellationToken ct)
    {
        // Collect module definitions
        var definitions = _modules
            .SelectMany(m => m.GetRoles().Select(r => (ModuleName: m.Name, Definition: r)))
            .ToList();

        // Include is critical for backing fields
        var databaseRoles = await writeDbContext.Roles.Include(r => r.Permissions).ToListAsync(ct);
        var databaseRolesMap = databaseRoles.ToDictionary(r => r.Name);
        var allDatabasePermissionsMap = allDatabasePermissions.ToDictionary(p => p.Name);

        // Upsert roles
        foreach (var (moduleName, definition) in definitions)
        {
            if (!databaseRolesMap.TryGetValue(definition.Name, out var role))
            {
                role = Role.Create(definition.Name, definition.Description, isModule: true, moduleName: moduleName);
                writeDbContext.Roles.Add(role);
            }
            else if (!role.IsModule)
            {
                role.MarkAsModuleRole(moduleName, definition.Description);
            }

            // Synchronize role permissions
            var requiredPermissions = definition.Permissions.Select(p => p.Name).ToHashSet();

            // Remove excessive permissions
            foreach (var permission in role.Permissions.Where(p => !requiredPermissions.Contains(p.Name)).ToList())
                role.RemovePermission(permission);

            // Add missing permissions
            foreach (var permissionName in requiredPermissions)
            {
                if (!role.Permissions.Any(p => p.Name == permissionName) && allDatabasePermissionsMap.TryGetValue(permissionName, out var databasePermission))
                    role.AddPermission(databasePermission);
            }
        }

        // Deactivate removed roles
        var definitionNames = definitions.Select(x => x.Definition.Name).ToHashSet();
        foreach (var role in databaseRoles.Where(r => r.IsModule && r.IsActive && !definitionNames.Contains(r.Name)))
            role.Deactivate();

        await writeDbContext.SaveChangesAsync(ct);
    }
}
