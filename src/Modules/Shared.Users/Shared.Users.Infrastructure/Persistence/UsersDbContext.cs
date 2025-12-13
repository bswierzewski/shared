using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Users module.
/// Implements <see cref="IUsersDbContext"/> for both read and write operations.
/// For read operations, use AsNoTracking() on queries for better performance.
///
/// Write operations (SaveChangesAsync) automatically audit changes:
/// - CreatedBy: Set from IUser (current user context)
/// - CreatedAt: Set to UtcNow
/// - ModifiedBy: Set from IUser
/// - ModifiedAt: Set to UtcNow
/// </summary>
public class UsersDbContext : DbContext, IUsersDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsersDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the DbContext.</param>
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the collection of users.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of roles.
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of permissions.
    /// </summary>
    public DbSet<Permission> Permissions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of external provider mappings.
    /// </summary>
    public DbSet<ExternalProvider> ExternalProviders { get; set; } = null!;

    /// <summary>
    /// Configures the model during context initialization.
    /// Applies entity configurations from this assembly and sets the PostgreSQL schema.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
