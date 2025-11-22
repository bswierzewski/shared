using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

        // Configure table name with explicit mapping to match aggregate root configuration
        builder.ToTable("User_Role");

        // Add index for performance when querying by RoleId
        builder.HasIndex(ur => ur.RoleId);
    }
}
