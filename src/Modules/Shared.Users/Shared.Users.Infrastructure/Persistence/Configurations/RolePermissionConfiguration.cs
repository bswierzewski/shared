using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Role-Permission many-to-many join table.
/// Explicitly configures the join table without exposing it as a DbSet.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    /// <summary>
    /// Configures the Role-Permission join entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        // Configure composite primary key
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Configure table name with explicit mapping to match aggregate root configuration
        builder.ToTable("Role_Permission");

        // Add index for performance when querying by PermissionId
        builder.HasIndex(rp => rp.PermissionId);
    }
}
