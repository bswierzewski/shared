using Shared.Users.Domain.Enums;

namespace Shared.Users.Domain.Entities;

/// <summary>
/// External identity provider mapping - represents a linked external identity for a user.
/// Entity: Has unique identifier and database persistence.
/// Referenced by User aggregate (One-to-Many relationship).
/// </summary>
public class ExternalProvider
{
    /// <summary>
    /// Unique identifier for this external provider mapping
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The User ID this external provider is linked to
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Identity provider (Auth0, Clerk, Google, etc)
    /// </summary>
    public IdentityProvider Provider { get; private set; }

    /// <summary>
    /// External user ID from the provider (e.g., "auth0|123456", "user_xyz789")
    /// </summary>
    public string ExternalUserId { get; private set; } = null!;

    /// <summary>
    /// When this external provider was linked to the user
    /// </summary>
    public DateTimeOffset AddedAt { get; private set; }

    private ExternalProvider() { }

    /// <summary>
    /// Factory method to create new external provider mapping
    /// </summary>
    public static ExternalProvider Create(Guid userId, IdentityProvider provider, string externalUserId)
    {
        return new ExternalProvider
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ExternalUserId = externalUserId,
            AddedAt = DateTimeOffset.UtcNow
        };
    }
}
