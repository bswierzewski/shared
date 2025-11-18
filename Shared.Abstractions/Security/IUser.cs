namespace Shared.Abstractions.Security;

/// <summary>
/// Interface representing the current authenticated user.
/// Provides access to user information extracted from JWT tokens (Auth0, Clerk, etc.).
/// </summary>
public interface IUser
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// Returns the internal database GUID from custom "user_id" claim added after JIT provisioning.
    /// Returns null if the user hasn't been provisioned yet (during initial authentication).
    /// Null in audit fields (CreatedBy/ModifiedBy) indicates system-created/modified entities.
    /// </summary>
    Guid? Id { get; }

    /// <summary>
    /// Gets the email address of the user from JWT claims.
    /// Can be null if email claim is not present in the token.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the full name of the user from JWT claims.
    /// Typically combines given name and family name, or uses the 'name' claim.
    /// Can be null if name claims are not present in the token.
    /// </summary>
    string? FullName { get; }

    /// <summary>
    /// Gets the URL to the user's profile picture from JWT claims.
    /// Uses the 'picture' claim (OpenID Connect standard, supported by Auth0, Clerk, etc.).
    /// Can be null if picture claim is not present in the token.
    /// </summary>
    string? PictureUrl { get; }

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// Returns true if a valid JWT token is present.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets all claims associated with the current user from the JWT token.
    /// </summary>
    IEnumerable<string> Claims { get; }

    /// <summary>
    /// Gets the roles assigned to the current user from JWT claims.
    /// Roles are typically stored in role or roles claims depending on the provider.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Gets the permissions assigned to the current user from JWT claims.
    /// Permissions provide fine-grained access control beyond role-based checks.
    /// Format may vary by provider (Auth0: custom namespace, Clerk: org:resource:action).
    /// </summary>
    IEnumerable<string> Permissions { get; }

    /// <summary>
    /// Determines whether the user has the specified role.
    /// </summary>
    /// <param name="role">The role to check for.</param>
    /// <returns>True if the user has the specified role; otherwise, false.</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Determines whether the user has the specified claim.
    /// </summary>
    /// <param name="claimType">The type of the claim to check for.</param>
    /// <param name="claimValue">The value of the claim to check for. If null, checks only for claim type existence.</param>
    /// <returns>True if the user has the specified claim; otherwise, false.</returns>
    bool HasClaim(string claimType, string? claimValue = null);

    /// <summary>
    /// Determines whether the user has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to check for (e.g., "create:posts", "delete:users").</param>
    /// <returns>True if the user has the specified permission; otherwise, false.</returns>
    bool HasPermission(string permission);
}