namespace Shared.Abstractions.Authorization;

/// <summary>
/// Represents a permission that can be granted to users.
/// Permissions provide fine-grained access control within the application.
/// </summary>
/// <param name="Name">The unique identifier for the permission (e.g., "users.view"). Should be module-prefixed.</param>
/// <param name="DisplayName">The human-readable name for the permission (e.g., "View Users").</param>
/// <param name="Module">The name of the module that defines this permission.</param>
/// <param name="Description">Description explaining what this permission allows.</param>
public sealed record Permission(
    string Name,
    string DisplayName,
    string Module,
    string Description);
