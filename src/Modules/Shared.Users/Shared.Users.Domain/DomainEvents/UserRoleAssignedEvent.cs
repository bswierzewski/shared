using Shared.Abstractions.Abstractions;

namespace Shared.Users.Domain.DomainEvents;

/// <summary>
/// Domain event fired when a role is assigned to a user
/// </summary>
public record UserRoleAssignedEvent(
    Guid UserId,
    string RoleName) : IDomainEvent
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
