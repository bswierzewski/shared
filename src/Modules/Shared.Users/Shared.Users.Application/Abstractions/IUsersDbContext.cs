using Microsoft.EntityFrameworkCore;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Application.Abstractions;

/// <summary>
/// Database context interface for User operations.
/// Provides access to user data with DbSet for both read and write operations.
/// Use AsNoTracking() in queries for read-only performance optimization.
/// </summary>
public interface IUsersDbContext
{
    /// <summary>
    /// Gets access to users.
    /// For read operations, use AsNoTracking() for better performance.
    /// </summary>
    DbSet<User> Users { get; }

    /// <summary>
    /// Gets access to external providers.
    /// Used when linking new external providers to users.
    /// </summary>
    DbSet<ExternalProvider> ExternalProviders { get; }

    /// <summary>
    /// Gets access to roles.
    /// For read operations, use AsNoTracking() for better performance.
    /// </summary>
    DbSet<Role> Roles { get; }

    /// <summary>
    /// Gets access to permissions.
    /// For read operations, use AsNoTracking() for better performance.
    /// </summary>
    DbSet<Permission> Permissions { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
