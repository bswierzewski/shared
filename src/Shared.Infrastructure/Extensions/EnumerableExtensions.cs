using Shared.Infrastructure.Models;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IEnumerable to provide additional functionality for collections.
/// These extensions help with common operations on enumerable collections.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an enumerable to a paginated list.
    /// This method loads all items into memory before applying pagination.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated list containing the specified page of items.</returns>
    public static PaginatedList<T> ToPaginatedList<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var list = source.ToList();
        var totalCount = list.Count;

        var skip = Math.Max(0, (pageNumber - 1) * pageSize);
        var items = list.Skip(skip).Take(pageSize);

        return new PaginatedList<T>([.. items], totalCount, pageNumber, pageSize);
    }
}