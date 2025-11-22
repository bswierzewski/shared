using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for User-Permission many-to-many join table.
/// Explicitly configures the join table without exposing it as a DbSet.
/// </summary>
public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    /// <summary>
    /// Configures the User-Permission join entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        // Configure composite primary key
        builder.HasKey(up => new { up.UserId, up.PermissionId });

        // Configure foreign key to User
        builder.HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure foreign key to Permission
        builder.HasOne(up => up.Permission)
            .WithMany()
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure table name
        builder.ToTable("User_Permission");

        // Add index for performance when querying by PermissionId
        builder.HasIndex(up => up.PermissionId);
    }
}
