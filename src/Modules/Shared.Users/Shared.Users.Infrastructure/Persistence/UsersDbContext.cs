using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Users module.
/// Implements both IUsersReadDbContext (no-tracking) and IUsersWriteDbContext (change-tracking).
///
/// Write operations (SaveChangesAsync) automatically audit changes:
/// - CreatedBy: Set from IUser (current user context)
/// - CreatedAt: Set to UtcNow
/// - ModifiedBy: Set from IUser
/// - ModifiedAt: Set to UtcNow
/// </summary>
/// <summary>
/// EF Core DbContext for the Users module.
/// Implements both <see cref="IUsersReadDbContext"/> (no-tracking queries) and <see cref="IUsersWriteDbContext"/> (change-tracked operations).
/// </summary>
public class UsersDbContext : DbContext, IUsersReadDbContext, IUsersWriteDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsersDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the DbContext.</param>
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    // Read operations (no-tracking for performance)
    IQueryable<User> IUsersReadDbContext.Users => Users.AsNoTracking();
    IQueryable<Role> IUsersReadDbContext.Roles => Roles.AsNoTracking();
    IQueryable<Permission> IUsersReadDbContext.Permissions => Permissions.AsNoTracking();

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
    /// Saves all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);

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
