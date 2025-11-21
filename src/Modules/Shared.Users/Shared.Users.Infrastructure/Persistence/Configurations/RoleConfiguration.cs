using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Role entity.
/// Represents a system-wide role definition with assigned permissions.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    /// <summary>
    /// Configures the Role entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.IsModule)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.ModuleName)
            .HasMaxLength(256);

        // Unique constraint on role name
        builder.HasIndex(r => r.Name)
            .IsUnique();

        // Index on ModuleName for filtering roles by module
        builder.HasIndex(r => r.ModuleName);

        // Index on IsActive for filtering active roles only
        builder.HasIndex(r => r.IsActive);

        // Index on IsModule for filtering module vs custom roles
        builder.HasIndex(r => r.IsModule);

        // Note: Role <-> User and Role <-> Permission relationships are configured in their respective
        // configuration classes (UserConfiguration and PermissionConfiguration) and will have
        // PropertyAccessMode.Field set there
    }
}
