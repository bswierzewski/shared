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
        // Name is the primary key (module-prefixed for global uniqueness)
        builder.HasKey(r => r.Name);

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

        // Index on ModuleName for filtering roles by module
        builder.HasIndex(r => r.ModuleName);

        // Index on IsActive for filtering active roles only
        builder.HasIndex(r => r.IsActive);

        // Index on IsModule for filtering module vs custom roles
        builder.HasIndex(r => r.IsModule);

        // Configure EF Core to use backing fields for navigation properties
        // This is crucial for Include() to work with IReadOnlyCollection properties
        // Note: Role <-> User and Role <-> Permission relationships are configured via
        // UserConfiguration and PermissionConfiguration respectively
        builder.Navigation(r => r.Users).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(r => r.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
