namespace Shared.Abstractions.Abstractions;

/// <summary>
/// Interface for entities that require audit tracking.
/// Provides properties to track when an entity was created and last modified, and by whom.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// Null indicates the entity was created by the system (e.g., during JIT user provisioning).
    /// </summary>
    Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// Can be null if the entity has never been modified.
    /// </summary>
    DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// Can be null if the entity has never been modified or was modified by the system.
    /// </summary>
    Guid ModifiedBy { get; set; }
}