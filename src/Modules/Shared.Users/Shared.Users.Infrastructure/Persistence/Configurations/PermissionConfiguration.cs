using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Permission entity.
/// Represents a system-wide permission definition that can be assigned to roles or users directly.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    /// <summary>
    /// Configures the Permission entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.IsModule)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.ModuleName)
            .HasMaxLength(256);

        // Unique constraint on permission name
        builder.HasIndex(p => p.Name)
            .IsUnique();

        // Index on ModuleName for filtering permissions by module
        builder.HasIndex(p => p.ModuleName);

        // Index on IsActive for filtering active permissions only
        builder.HasIndex(p => p.IsActive);

        // Index on IsModule for filtering module vs custom permissions
        builder.HasIndex(p => p.IsModule);

        // Many-to-Many: Permission <-> Role (detailed configuration in RolePermissionConfiguration)
        builder.HasMany(p => p.Roles)
            .WithMany(r => r.Permissions)
            .UsingEntity<RolePermission>();

        // Note: Permission <-> User relationship is already configured in UserConfiguration
        // to avoid defining the same relationship from both sides (Single Source of Truth)

        // Configure EF Core to use backing fields for navigation properties
        // This is crucial for Include() to work with IReadOnlyCollection properties
        builder.Navigation(p => p.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
