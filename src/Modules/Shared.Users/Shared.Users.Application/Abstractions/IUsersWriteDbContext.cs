using Microsoft.EntityFrameworkCore;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Application.Abstractions;

/// <summary>
/// Write database context interface for User command operations.
/// Provides access to user data with change tracking.
/// Part of CQRS pattern: separated read and write contexts.
/// </summary>
public interface IUsersWriteDbContext
{
    /// <summary>
    /// Gets write access to users with change tracking.
    /// </summary>
    DbSet<User> Users { get; }

    /// <summary>
    /// Gets write access to roles with change tracking.
    /// </summary>
    DbSet<Domain.Entities.Role> Roles { get; }

    /// <summary>
    /// Gets write access to permissions with change tracking.
    /// </summary>
    DbSet<Domain.Entities.Permission> Permissions { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
