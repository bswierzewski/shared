using Shared.Users.Domain.Aggregates;

namespace Shared.Users.Domain.Entities;

/// <summary>
/// Join entity for User-Role many-to-many relationship.
/// This is not exposed as a DbSet but configured explicitly via UserRoleConfiguration.
/// </summary>
public class UserRole
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
    /// Foreign key to Role
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    public Role Role { get; set; } = null!;
}
