namespace Shared.Abstractions.Primitives;

/// <summary>
/// Base class for entities with domain event support.
/// Entities are objects that have a distinct identity that runs through time and different representations.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Entity{TId}"/> class.
/// </remarks>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public TId Id { get; init; } = default!;
}