using Testcontainers.PostgreSql;

namespace Shared.Infrastructure.Tests.Infrastructure.Containers;

/// <summary>
/// PostgreSQL test container implementation.
/// Manages the lifecycle of a PostgreSQL container for integration tests.
/// </summary>
public class PostgreSqlTestContainer : ITestContainer
{
    private readonly PostgreSqlContainer _container;

    /// <summary>
    /// Initializes a new instance of PostgreSQL test container with default configuration.
    /// Uses postgres:16 image with test credentials.
    /// </summary>
    public PostgreSqlTestContainer()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithDatabase("postgres")
            .Build();
    }

    /// <summary>
    /// Starts the PostgreSQL container.
    /// </summary>
    public Task StartAsync() => _container.StartAsync();

    /// <summary>
    /// Stops and disposes the PostgreSQL container.
    /// </summary>
    public Task StopAsync() => _container.DisposeAsync().AsTask();

    /// <summary>
    /// Gets the connection string for the PostgreSQL container.
    /// </summary>
    public string GetConnectionString() => _container.GetConnectionString();
}
