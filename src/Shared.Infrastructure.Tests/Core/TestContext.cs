using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Tests.Builders;

namespace Shared.Infrastructure.Tests.Core;

/// <summary>
/// Primary test context providing access to test infrastructure.
/// Use TestContext.CreateBuilder&lt;TProgram&gt;() to configure and build.
/// </summary>
/// <remarks>
/// This class is sealed to encourage composition over inheritance.
/// For domain-specific helpers, create extension methods instead of inheriting from this class.
/// </remarks>
public sealed class TestContext : IAsyncDisposable
{
    private readonly ITestHost _host;
    private HttpClient? _client;

    internal TestContext(ITestHost host)
    {
        _host = host;
    }

    /// <summary>
    /// Gets the HTTP client for making requests to the test application.
    /// Client is created lazily on first access.
    /// </summary>
    public HttpClient Client => _client ??= _host.CreateClient();

    /// <summary>
    /// Gets the service provider for resolving services from the test application.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    /// <summary>
    /// Resets the database to a clean state.
    /// Typically called before each test to ensure test isolation.
    /// </summary>
    public Task ResetDatabaseAsync() => _host.ResetDatabaseAsync();

    /// <summary>
    /// Creates a new service scope for resolving scoped services.
    /// Remember to dispose the scope when done.
    /// </summary>
    public IServiceScope CreateScope() => Services.CreateScope();

    /// <summary>
    /// Resolves a required service from the test service provider.
    /// Throws if the service is not registered.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    public T GetRequiredService<T>() where T : notnull
        => Services.GetRequiredService<T>();

    /// <summary>
    /// Resolves an optional service from the test service provider.
    /// Returns null if the service is not registered.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance or null.</returns>
    public T? GetService<T>() where T : class
        => Services.GetService<T>();

    /// <summary>
    /// Disposes the test context and all associated resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        await _host.DisposeAsync();
    }

    /// <summary>
    /// Creates a new test context builder for the specified program type.
    /// This is the entry point for configuring and building test contexts.
    /// </summary>
    /// <typeparam name="TProgram">The Program or Startup class of the application under test.</typeparam>
    /// <returns>A new test context builder.</returns>
    /// <example>
    /// <code>
    /// var context = await TestContext.CreateBuilder&lt;Program&gt;()
    ///     .WithPostgreSql()
    ///     .BuildAsync();
    /// </code>
    /// </example>
    public static TestContextBuilder<TProgram> CreateBuilder<TProgram>()
        where TProgram : class
        => new TestContextBuilder<TProgram>();
}
