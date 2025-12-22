namespace Shared.Abstractions.Abstractions;

/// <summary>
/// Represents the currently authenticated user and their authorization context.
/// This interface provides access to user identity, roles, permissions, and claims.
/// </summary>
public interface IUser
{
    /// <summary>
    /// Gets the internal user ID (GUID).
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets all roles assigned to the user.
    /// Includes roles from both direct assignment and role claims.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Gets all permissions granted to the user.
    /// Includes both direct permissions and permissions from assigned roles.
    /// </summary>
    IEnumerable<string> Permissions { get; }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="role">The role name to check.</param>
    /// <returns>True if the user has the specified role; otherwise, false.</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission name to check.</param>
    /// <returns>True if the user has the specified permission; otherwise, false.</returns>
    bool HasPermission(string permission);
}
