using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Aggregates;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for User aggregate root
/// </summary>
internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.DisplayName)
            .HasMaxLength(500);

        builder.Property(u => u.PictureUrl);

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

        // Many-to-Many: User <-> Role (via User_Role table)
        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity("User_Role");

        // Many-to-Many: User <-> Permission (via User_Permission table)
        builder.HasMany(u => u.Permissions)
            .WithMany(p => p.Users)
            .UsingEntity("User_Permission");
    }
}
