namespace Shared.Abstractions.Security;

/// <summary>
/// Represents a permission that can be granted to users.
/// Permissions provide fine-grained access control within the application.
/// </summary>
/// <param name="Name">The unique identifier for the permission (e.g., "users.create").</param>
/// <param name="DisplayName">The human-readable name for the permission (e.g., "Create Users").</param>
/// <param name="Module">The name of the module that defines this permission.</param>
/// <param name="Description">Optional description explaining what this permission allows.</param>
public sealed record Permission(
    string Name,
    string DisplayName,
    string Module,
    string? Description = null)
{
    /// <summary>
    /// Creates a new Permission instance.
    /// </summary>
    /// <param name="name">The unique identifier for the permission.</param>
    /// <param name="displayName">The human-readable name.</param>
    /// <param name="module">The module that owns this permission.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new Permission instance.</returns>
    public static Permission Create(string name, string displayName, string module, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(module);

        return new Permission(name, displayName, module, description);
    }
}
