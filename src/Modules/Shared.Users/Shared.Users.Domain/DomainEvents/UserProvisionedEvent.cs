using Shared.Abstractions.Abstractions;
using Shared.Users.Domain.Enums;

namespace Shared.Users.Domain.DomainEvents;

/// <summary>
/// Domain event fired when a new user is provisioned (JIT user creation)
/// </summary>
public record UserProvisionedEvent(
    Guid UserId,
    string Email,
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
