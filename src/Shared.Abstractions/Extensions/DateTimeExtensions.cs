namespace Shared.Abstractions.Extensions;

/// <summary>
/// Provides extension methods for DateTime operations to support order processing workflows.
/// These utilities are designed to facilitate date range operations commonly used
/// in order retrieval and batch processing scenarios.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Generates a sequence of consecutive dates within the specified range (inclusive).
    /// This method is particularly useful for splitting date ranges into individual days
    /// for parallel processing of external service calls, which improves performance
    /// when retrieving orders from external systems like Polcar.
    /// </summary>
    /// <param name="from">The start date of the range (inclusive)</param>
    /// <param name="to">The end date of the range (inclusive)</param>
    /// <returns>An enumerable sequence of DateTime objects representing each day in the range</returns>
    public static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
    {
        // Ensure we work with date-only values to avoid time component issues
        // This normalizes input dates and prevents unexpected behavior with time portions
        for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
        {
            // Yield return enables lazy evaluation, which is memory-efficient
            // for large date ranges and allows consumers to break early if needed
            yield return day;
        }
    }
}