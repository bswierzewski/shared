using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MediatR;
using Shared.Abstractions.Abstractions;

namespace Shared.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically publishes domain events after SaveChanges.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DispatchDomainEventsInterceptor"/> class.
/// </remarks>
/// <param name="mediator">The MediatR publisher.</param>
public sealed class DispatchDomainEventsInterceptor(IPublisher mediator) : SaveChangesInterceptor
{
    private readonly IPublisher _mediator = mediator;

    /// <summary>
    /// Intercepts SaveChanges calls.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts SaveChangesAsync calls.
    /// </summary>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Collects and publishes all domain events from entities with domain events.
    /// </summary>
    private async Task DispatchDomainEvents(DbContext? context)
    {
        if (context is null)
            return;

        var entities = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity);

        var domainEvents = entities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        // Clear domain events from entities before publishing
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        // Publish all domain events
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
    }
}