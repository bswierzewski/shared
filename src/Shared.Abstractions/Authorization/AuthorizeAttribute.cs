namespace Shared.Abstractions.Authorization;

/// <summary>
/// Attribute used to declare authorization requirements for MediatR requests.
/// Applied to request record classes to specify required roles, permissions, and claims.
/// </summary>
/// <remarks>
/// Authorization checks are performed by AuthorizationBehavior{TRequest, TResponse} in the MediatR pipeline.
/// Multiple authorization requirements are combined:
/// - Roles: User must have ALL specified permissions (AND logic)
/// - Permissions: User must have ALL specified permissions (AND logic)
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the required roles.
    /// User must have all of these permissions. AND logic applies.
    /// </summary>
    public string[] Roles { get; set; } = []; 

    /// <summary>
    /// Gets or sets the required permissions.
    /// User must have all of these permissions. AND logic applies.
    /// </summary>
    public string[] Permissions { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class.
    /// </summary>
    public AuthorizeAttribute() { }
}
