namespace Shared.Infrastructure.Tests.Factories;

/// <summary>
/// Interface for test web application factories that manage test databases and lifecycle.
/// </summary>
public interface ITestWebApplicationFactory
{
    /// <summary>
    /// Gets the service provider for the test application.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// Creates an HTTP client for testing.
    /// </summary>
    HttpClient CreateClient();

    /// <summary>
    /// Resets all test databases to a clean state.
    /// </summary>
    Task ResetDatabasesAsync();

    /// <summary>
    /// Configures test services and returns a new factory instance.
    /// </summary>
    ITestWebApplicationFactory WithWebHostBuilder(Action<Microsoft.AspNetCore.Hosting.IWebHostBuilder> configure);
}
