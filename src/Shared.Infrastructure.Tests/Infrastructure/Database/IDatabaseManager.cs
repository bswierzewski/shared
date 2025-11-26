namespace Shared.Infrastructure.Tests.Infrastructure.Database;

/// <summary>
/// Abstraction for database management in tests.
/// Provides operations for database reset and cleanup.
/// </summary>
public interface IDatabaseManager
{
    /// <summary>
    /// Resets the database to a clean state using the specified strategy.
    /// </summary>
    /// <param name="strategy">The reset strategy to use.</param>
    Task ResetAsync(DatabaseResetStrategy strategy);
}
