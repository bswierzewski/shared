using Shared.Abstractions.Abstractions;

namespace Shared.Abstractions.Primitives;

/// <summary>
/// Base class for domain events that implements common functionality.
/// Domain events represent significant occurrences within the domain that other parts
/// of the system may need to react to.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when the domain event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}