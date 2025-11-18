namespace Shared.Abstractions.Security;

/// <summary>
/// Represents a role that groups permissions together.
/// Roles provide a way to assign multiple permissions to users at once.
/// </summary>
/// <param name="Name">The unique identifier for the role (e.g., "users.admin").</param>
/// <param name="DisplayName">The human-readable name for the role (e.g., "Users Administrator").</param>
/// <param name="Module">The name of the module that defines this role.</param>
/// <param name="Permissions">The collection of permissions assigned to this role.</param>
/// <param name="Description">Optional description explaining the purpose of this role.</param>
public sealed record Role(
    string Name,
    string DisplayName,
    string Module,
    IReadOnlyCollection<Permission> Permissions,
    string? Description = null)
{
    /// <summary>
    /// Creates a new Role instance.
    /// </summary>
    /// <param name="name">The unique identifier for the role.</param>
    /// <param name="displayName">The human-readable name.</param>
    /// <param name="module">The module that owns this role.</param>
    /// <param name="permissions">The permissions assigned to this role.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new Role instance.</returns>
    public static Role Create(
        string name,
        string displayName,
        string module,
        IEnumerable<Permission> permissions,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(module);
        ArgumentNullException.ThrowIfNull(permissions);

        return new Role(name, displayName, module, permissions.ToList().AsReadOnly(), description);
    }

    /// <summary>
    /// Creates a new Role instance with no permissions.
    /// </summary>
    /// <param name="name">The unique identifier for the role.</param>
    /// <param name="displayName">The human-readable name.</param>
    /// <param name="module">The module that owns this role.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new Role instance with empty permissions.</returns>
    public static Role CreateEmpty(
        string name,
        string displayName,
        string module,
        string? description = null)
    {
        return Create(name, displayName, module, Enumerable.Empty<Permission>(), description);
    }
}
