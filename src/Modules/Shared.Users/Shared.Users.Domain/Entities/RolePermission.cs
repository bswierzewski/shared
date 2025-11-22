namespace Shared.Users.Domain.Entities;

/// <summary>
/// Join entity for Role-Permission many-to-many relationship.
/// This is not exposed as a DbSet but configured explicitly via RolePermissionConfiguration.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Foreign key to Role
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Foreign key to Permission
    /// </summary>
    public Guid PermissionId { get; set; }
}
