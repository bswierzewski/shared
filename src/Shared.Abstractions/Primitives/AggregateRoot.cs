using Shared.Abstractions.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Abstractions.Primitives;

/// <inheritdoc/>
public abstract class AggregateRoot<TId> : AuditableEntity<TId>, IAggregateRoot
    where TId : notnull
{    
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc/>
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc/>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <inheritdoc/>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <inheritdoc/>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}