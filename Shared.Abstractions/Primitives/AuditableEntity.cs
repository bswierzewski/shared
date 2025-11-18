using Shared.Abstractions.Abstractions;

namespace Shared.Abstractions.Primitives;

/// <summary>
/// Base class for auditable entities that tracks creation and modification metadata.
/// This class extends the base entity functionality with audit trail capabilities.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AuditableEntity{TId}"/> class.
/// </remarks>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    where TId : notnull
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// Null indicates the entity was created by the system (e.g., during JIT user provisioning).
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// Can be null if the entity has never been modified.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// Can be null if the entity has never been modified or was modified by the system.
    /// </summary>
    public Guid? ModifiedBy { get; set; }
}