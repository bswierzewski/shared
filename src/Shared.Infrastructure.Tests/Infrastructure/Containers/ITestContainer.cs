namespace Shared.Infrastructure.Tests.Infrastructure.Containers;

/// <summary>
/// Abstraction for test containers (e.g., PostgreSQL, Redis, etc.).
/// Provides lifecycle management and connection information.
/// </summary>
public interface ITestContainer
{
    /// <summary>
    /// Starts the test container.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stops and disposes the test container.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Gets the connection string for the test container.
    /// </summary>
    string GetConnectionString();
}
