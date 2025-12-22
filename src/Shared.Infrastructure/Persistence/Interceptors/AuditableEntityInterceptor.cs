using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Abstractions.Abstractions;

namespace Shared.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically sets audit fields on entities that implement IAuditable.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuditableEntityInterceptor"/> class.
/// </remarks>
/// <param name="user">The current user (nullable for scenarios without authentication).</param>
/// <param name="dateTime">The time provider for consistent time handling.</param>
public sealed class AuditableEntityInterceptor(IUser user, TimeProvider dateTime) : SaveChangesInterceptor
{
    private readonly TimeProvider _dateTime = dateTime;

    /// <summary>
    /// Intercepts SaveChanges calls to update audit fields.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts SaveChangesAsync calls to update audit fields.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Updates audit fields on entities based on their state and owned entity changes.
    ///
    /// IMPORTANT: During JIT (Just-In-Time) user provisioning:
    /// - IUser.Id returns null because ClaimTypes.NameIdentifier (external ID) was removed before provisioning
    /// - CreatedBy/ModifiedBy will be set to null, indicating system-created entities
    /// - After JIT provisioning completes, ClaimTypes.NameIdentifier is replaced with internal GUID
    /// - All subsequent operations will use the internal GUID for audit tracking
    /// </summary>
    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = _dateTime.GetUtcNow();

                if (entry.State == EntityState.Added)
                {
                    // During JIT provisioning, _user.Id will be null (system-created)
                    // After provisioning, it will be the internal GUID
                    entry.Entity.CreatedBy = user.Id;
                    entry.Entity.CreatedAt = utcNow;
                }

                // For modifications, _user.Id will be the internal GUID
                entry.Entity.ModifiedBy = user.Id;
                entry.Entity.ModifiedAt = utcNow;
            }
        }
    }
}

/// <summary>
/// Extension methods for EntityEntry to support owned entity change detection.
/// </summary>
public static class EntityEntryExtensions
{
    /// <summary>
    /// Determines whether the entity has changed owned entities.
    /// Owned entities are value objects that are stored in the same table as the owning entity.
    /// </summary>
    /// <param name="entry">The entity entry to check.</param>
    /// <returns>True if any owned entities have been added or modified; otherwise, false.</returns>
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}