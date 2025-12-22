using Shared.Abstractions.Abstractions;

namespace Shared.Abstractions.Primitives;

/// <inheritdoc/>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    where TId : notnull
{
    /// <inheritdoc/>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public Guid CreatedBy { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <inheritdoc/>
    public Guid ModifiedBy { get; set; }
}