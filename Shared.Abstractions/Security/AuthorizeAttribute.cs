namespace Shared.Abstractions.Security;

/// <summary>
/// Attribute to specify authorization requirements for MediatR requests.
/// Supports role-based, permission-based, and claim-based authorization using JWT tokens.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the required roles (comma-separated).
    /// The user must have at least one of the specified roles (OR logic).
    /// </summary>
    /// <example>"Admin, SuperAdmin"</example>
    public string? Roles { get; set; }

    /// <summary>
    /// Gets or sets the required permissions (comma-separated).
    /// The user must have all specified permissions (AND logic).
    /// </summary>
    /// <example>"users.view, users.edit"</example>
    public string? Permissions { get; set; }

    /// <summary>
    /// Gets or sets the required claims (comma-separated).
    /// Format: "claimType" or "claimType:claimValue".
    /// The user must have all specified claims (AND logic).
    /// </summary>
    /// <example>"email_verified:true, country:US"</example>
    public string? Claims { get; set; }

    /// <summary>
    /// Initializes a new instance of the AuthorizeAttribute class.
    /// </summary>
    public AuthorizeAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuthorizeAttribute class with required roles.
    /// </summary>
    /// <param name="roles">The required roles (comma-separated).</param>
    public AuthorizeAttribute(string roles)
    {
        Roles = roles;
    }
}