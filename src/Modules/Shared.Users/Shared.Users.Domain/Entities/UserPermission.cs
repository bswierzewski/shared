using Shared.Users.Domain.Aggregates;

namespace Shared.Users.Domain.Entities;

/// <summary>
/// Join entity for User-Permission many-to-many relationship.
/// This is not exposed as a DbSet but configured explicitly via UserPermissionConfiguration.
/// </summary>
public class UserPermission
{
    /// <summary>
    /// Foreign key to User
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Foreign key to Permission
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Navigation property to Permission
    /// </summary>
    public Permission Permission { get; set; } = null!;
}
