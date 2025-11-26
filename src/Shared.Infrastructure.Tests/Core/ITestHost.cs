namespace Shared.Infrastructure.Tests.Core;

/// <summary>
/// Abstraction for test application host.
/// Provides access to the service provider, HTTP client creation, and database reset functionality.
/// </summary>
public interface ITestHost : IAsyncDisposable
{
    /// <summary>
    /// Gets the service provider for the test application.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// Creates an HTTP client for making requests to the test application.
    /// </summary>
    HttpClient CreateClient();

    /// <summary>
    /// Resets the database to a clean state.
    /// </summary>
    Task ResetDatabaseAsync();
}
