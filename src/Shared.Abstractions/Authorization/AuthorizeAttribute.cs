namespace Shared.Abstractions.Authorization;

/// <summary>
/// Attribute used to declare authorization requirements for MediatR requests.
/// Applied to request record classes to specify required roles, permissions, and claims.
/// </summary>
/// <remarks>
/// Authorization checks are performed by AuthorizationBehavior{TRequest, TResponse} in the MediatR pipeline.
/// Multiple authorization requirements are combined:
/// - Roles: User must have at least ONE of the specified roles (OR logic)
/// - Permissions: User must have ALL specified permissions (AND logic)
/// - Claims: User must have ALL specified claims (AND logic)
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the required roles (comma-separated).
    /// User must have at least one of these roles. OR logic applies.
    /// Example: "admin,moderator" means user must have either admin or moderator role.
    /// </summary>
    public string? Roles { get; set; }

    /// <summary>
    /// Gets or sets the required permissions (comma-separated).
    /// User must have all of these permissions. AND logic applies.
    /// Example: "users.view,users.edit" means user must have both permissions.
    /// </summary>
    public string? Permissions { get; set; }

    /// <summary>
    /// Gets or sets the required claims (comma-separated).
    /// User must have all of these claims. AND logic applies.
    /// Format: "claimType" or "claimType:claimValue"
    /// Example: "department:sales" means user must have a claim with type "department" and value "sales".
    /// </summary>
    public string? Claims { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class.
    /// </summary>
    public AuthorizeAttribute()
    {
    }
}
