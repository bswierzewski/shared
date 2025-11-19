using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ExternalProvider entity.
/// Represents mapping between internal User ID and external identity provider ID.
/// Stored in separate table with foreign key to User.
/// </summary>
internal class ExternalProviderConfiguration : IEntityTypeConfiguration<ExternalProvider>
{
    /// <summary>
    /// Configures the ExternalProvider entity.
    /// </summary>
    /// <param name="builder">The entity builder for ExternalProvider.</param>
    public void Configure(EntityTypeBuilder<ExternalProvider> builder)
    {
        // Primary key
        builder.HasKey(ep => ep.Id);

        // Foreign key to User
        builder.Property(ep => ep.UserId)
            .IsRequired();

        // External ID from provider (e.g., Auth0 user ID)
        builder.Property(ep => ep.ExternalUserId)
            .IsRequired()
            .HasMaxLength(500);

        // Provider enum (Auth0, Clerk, Google, Microsoft, Other)
        builder.Property(ep => ep.Provider)
            .IsRequired();

        // When the provider was added to this user account
        builder.Property(ep => ep.AddedAt)
            .IsRequired();

        // Index on UserId for queries filtering by user
        builder.HasIndex(ep => ep.UserId);

        // Composite unique constraint: (Provider, ExternalUserId)
        // Prevents same external ID being linked to multiple users
        builder.HasIndex(ep => new { ep.Provider, ep.ExternalUserId })
            .IsUnique();
    }
}
