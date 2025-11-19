namespace Shared.Abstractions.Abstractions;

/// <summary>
/// Defines behavior for entities that manage domain events.
/// This interface is used for entities that track and publish domain events.
/// </summary>
public interface IHasDomainEvent
{
    /// <summary>
    /// Gets the read-only collection of domain events
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Adds a domain event to the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to add</param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Removes a specific domain event from the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to remove</param>
    void RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Clears all domain events from the aggregate's event collection
    /// </summary>
    void ClearDomainEvents();
}
