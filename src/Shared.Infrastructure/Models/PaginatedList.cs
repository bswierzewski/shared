namespace Shared.Infrastructure.Models;

/// <summary>
/// Represents a paginated list of items with metadata about the pagination.
/// Provides information about the current page, total count, and navigation capabilities.
/// </summary>
/// <typeparam name="T">The type of items in the paginated list.</typeparam>
/// <remarks>
/// Initializes a new instance of the PaginatedList class.
/// </remarks>
/// <param name="items">The items on the current page.</param>
/// <param name="count">The total number of items across all pages.</param>
/// <param name="pageNumber">The current page number (1-based).</param>
/// <param name="pageSize">The number of items per page.</param>
public class PaginatedList<T>(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
{
    /// <summary>
    /// Gets the items on the current page.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; } = items;

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; } = pageNumber;

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; } = (int)Math.Ceiling(count / (double)pageSize);

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; } = count;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

}