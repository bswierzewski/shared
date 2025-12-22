using Shared.Abstractions.Abstractions;

namespace Shared.Abstractions.Primitives;

/// <inheritdoc/>
public abstract class DomainEvent : IDomainEvent
{
    /// <inheritdoc/>
    public Guid Id { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}