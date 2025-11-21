using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for User aggregate root
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures the User entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.LastLoginAt);

        // Unique constraint on email
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Configure relationships

        // One-to-Many: User -> ExternalProvider (separate table)
        builder.HasMany(u => u.ExternalProviders)
            .WithOne()
            .HasForeignKey(ep => ep.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many: User <-> Role (configured via UserRole join entity)
        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<UserRole>(
                l => l.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId),
                r => r.HasOne(ur => ur.User).WithMany().HasForeignKey(ur => ur.UserId),
                j => j.HasKey(ur => new { ur.UserId, ur.RoleId }));

        // Many-to-Many: User <-> Permission (configured via UserPermission join entity)
        builder.HasMany(u => u.Permissions)
            .WithMany(p => p.Users)
            .UsingEntity<UserPermission>(
                l => l.HasOne(up => up.Permission).WithMany().HasForeignKey(up => up.PermissionId),
                r => r.HasOne(up => up.User).WithMany().HasForeignKey(up => up.UserId),
                j => j.HasKey(up => new { up.UserId, up.PermissionId }));

        // Configure EF Core to use backing fields for navigation properties
        // This is crucial for Include() to work with IReadOnlyCollection properties
        builder.Navigation(u => u.ExternalProviders).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
