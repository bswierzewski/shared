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
    /// Foreign key to Permission
    /// </summary>
    public Guid PermissionId { get; set; }
}
