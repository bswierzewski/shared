using System.ComponentModel.DataAnnotations.Schema;
using Shared.Abstractions.Abstractions;
using Shared.Abstractions.Exceptions;

namespace Shared.Abstractions.Primitives;

/// <summary>
/// Base class for aggregate root entities in Domain-Driven Design.
/// Aggregate roots are the only entities within an aggregate that can be referenced from outside the aggregate boundary.
/// They are responsible for maintaining the consistency and invariants of the entire aggregate.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
/// </remarks>
public abstract class AggregateRoot<TId> : AuditableEntity<TId>, IAggregateRoot
    where TId : notnull
{
    /// <summary>
    /// Private collection storing domain events for this aggregate
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the read-only collection of domain events. NotMapped to exclude from database persistence
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to add</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event from the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to remove</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the aggregate's event collection
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Checks a business rule and throws an exception if it's broken
    /// </summary>
    /// <param name="rule">The business rule to check</param>
    /// <exception cref="BusinessRuleValidationException">Thrown when the business rule is broken</exception>
    protected void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}