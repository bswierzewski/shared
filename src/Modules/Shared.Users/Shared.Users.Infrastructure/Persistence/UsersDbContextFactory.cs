using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shared.Users.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <see cref="UsersDbContext"/>.
/// Used by Entity Framework Core tools (dotnet ef) to create a DbContext instance
/// without requiring a full dependency injection container at design time.
/// </summary>
public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    /// <summary>
    /// Creates a DbContext instance for design-time operations (migrations, scaffolding, etc).
    /// Uses a placeholder connection string - actual connection is not needed for migration generation.
    /// The DbContext is only used to analyze the model schema during 'dotnet ef migrations' commands.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the design-time tool (typically unused).</param>
    /// <returns>A new <see cref="UsersDbContext"/> instance configured for EF Core migrations and scaffolding.</returns>
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        // Placeholder connection string - not used for migration generation
        optionsBuilder.UseNpgsql("Server=placeholder;Database=placeholder;User Id=placeholder;Password=placeholder");

        return new UsersDbContext(optionsBuilder.Options);
    }
}
