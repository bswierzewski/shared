using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for User-Role many-to-many join table.
/// Explicitly configures the join table without exposing it as a DbSet.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    /// <summary>
    /// Configures the User-Role join entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Configure composite primary key
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        // Configure foreign key to User
        builder.HasOne(ur => ur.User)
            .WithMany()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure foreign key to Role (maps to Role.Users collection via backing field)
        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure table name
        builder.ToTable("User_Role");

        // Add index for performance when querying by RoleId
        builder.HasIndex(ur => ur.RoleId);
    }
}
