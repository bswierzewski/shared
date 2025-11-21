using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Role-Permission many-to-many join table.
/// Explicitly configures the join table mapping.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    /// <summary>
    /// Configures the Role-Permission many-to-many relationship.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Configure many-to-many relationship
        builder.HasMany(p => p.Roles)
            .WithMany(r => r.Permissions)
            .UsingEntity(
                "RolePermission",
                l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne(typeof(Permission)).WithMany().HasForeignKey("PermissionId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey("RoleId", "PermissionId"));
    }
}
