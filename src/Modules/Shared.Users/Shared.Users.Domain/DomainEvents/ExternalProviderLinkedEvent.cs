using Shared.Abstractions.Abstractions;
using Shared.Users.Domain.Enums;

namespace Shared.Users.Domain.DomainEvents;

/// <summary>
/// Domain event fired when an external provider is linked to an existing user
/// </summary>
public record ExternalProviderLinkedEvent(
    Guid UserId,
    IdentityProvider Provider) : IDomainEvent
{
    /// <summary>
    /// Unique identifier of this domain event
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Date and time when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
