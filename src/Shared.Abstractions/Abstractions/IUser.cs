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
    string? Email { get; }

    /// <summary>
    /// Gets the user's full name or display name.
    /// </summary>
    string? FullName { get; }
}
