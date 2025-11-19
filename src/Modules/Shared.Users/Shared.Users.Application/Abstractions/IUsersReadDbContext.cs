using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Entities;

namespace Shared.Users.Application.Abstractions;

/// <summary>
/// Read-only database context interface for User queries.
/// Provides access to user data with no-tracking for performance optimization.
/// Part of CQRS pattern: separated read and write contexts.
/// </summary>
public interface IUsersReadDbContext
{
    /// <summary>
    /// Gets read-only access to users with no tracking.
    /// </summary>
    IQueryable<User> Users { get; }

    /// <summary>
    /// Gets read-only access to roles with no tracking.
    /// </summary>
    IQueryable<Domain.Entities.Role> Roles { get; }

    /// <summary>
    /// Gets read-only access to permissions with no tracking.
    /// </summary>
    IQueryable<Domain.Entities.Permission> Permissions { get; }
}
