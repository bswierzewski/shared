namespace Shared.Infrastructure.Tests.Infrastructure.Database;

/// <summary>
/// Default implementation of database management for tests.
/// Coordinates database reset operations using Respawn.
/// </summary>
public class DatabaseManager : IDatabaseManager
{
    /// <summary>
    /// Resets the database using the specified strategy.
    /// </summary>
    /// <param name="strategy">The reset strategy containing Respawn configuration.</param>
    public async Task ResetAsync(DatabaseResetStrategy strategy)
    {
        await strategy.ResetAsync();
    }
}
