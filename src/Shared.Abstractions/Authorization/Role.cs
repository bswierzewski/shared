namespace Shared.Abstractions.Authorization;

/// <summary>
/// Represents a role that groups permissions together.
/// Roles provide a way to assign multiple permissions to users at once.
/// </summary>
/// <param name="Name">The unique identifier for the role (e.g., "users.admin"). Should be module-prefixed.</param>
/// <param name="DisplayName">The human-readable name for the role (e.g., "Users Administrator").</param>
/// <param name="Module">The name of the module that defines this role.</param>
/// <param name="Description">Description explaining the purpose of this role.</param>
/// <param name="Permissions">The collection of permissions assigned to this role.</param>
public sealed record Role(
    string Name,
    string DisplayName,
    string Module,
    string Description,
    IReadOnlyCollection<Permission> Permissions);