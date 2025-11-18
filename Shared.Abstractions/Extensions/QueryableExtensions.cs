using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Models;

namespace Shared.Abstractions.Extensions;

/// <summary>
/// Extension methods for IQueryable to provide additional functionality for database queries.
/// These extensions help with common operations on queryable collections.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Creates a paginated list asynchronously from an IQueryable source.
    /// This method efficiently queries only the required page data from the database.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the queryable.</typeparam>
    /// <param name="source">The queryable source to paginate.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the paginated list.</returns>
    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}