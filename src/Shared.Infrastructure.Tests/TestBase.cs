using Shared.Infrastructure.Tests.Extensions;
using Shared.Infrastructure.Tests.Factories;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Shared.Infrastructure.Tests;

/// <summary>
/// Base class for integration tests providing common setup, cleanup, and utility methods.
/// Manages test isolation through database resets and service configuration hooks.
/// </summary>
public abstract class TestBase(ITestWebApplicationFactory factory) : IAsyncLifetime
{
    // Fields
    private readonly ITestWebApplicationFactory _factory = factory;

    // Properties
    /// <summary>
    /// Gets the HTTP client for making requests to the test application.
    /// </summary>
    protected HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// Gets the service provider for resolving services from the test application.
    /// </summary>
    protected IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// Override this property to return true if the test requires service customization via OnConfigureServices.
    /// When false (default), the shared factory is used for better performance.
    /// </summary>
    protected virtual bool RequiresServiceCustomization => false;

    /// <summary>
    /// Initializes the test by resetting databases and creating an HTTP client.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _factory.ResetDatabasesAsync();

        // Performance optimization: Only create customized factory when needed.
        // Most tests don't need service customization and can use the shared factory,
        // which avoids re-initializing the application.
        var factory = RequiresServiceCustomization
            ? _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(OnConfigureServices))
            : _factory;

        Client = factory.CreateClient();
        Services = factory.Services;

        await OnInitializeAsync();
    }

    /// <summary>
    /// Disposes test resources asynchronously.
    /// </summary>
    public virtual Task DisposeAsync() => Task.CompletedTask;

    // Protected virtual methods (hooks for derived classes)
    /// <summary>
    /// Override to configure additional or replacement services for the test.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    protected virtual void OnConfigureServices(IServiceCollection services) { }

    /// <summary>
    /// Override to perform additional initialization after the test client is created.
    /// </summary>
    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    // Protected utility methods
    /// <summary>
    /// Creates a new service scope for resolving scoped services.
    /// </summary>
    /// <returns>A disposable service scope.</returns>
    protected IServiceScope CreateScope() => Services.CreateScope();

    /// <summary>
    /// Resolves a required service from the test service provider.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    protected T Resolve<T>() where T : notnull => Services.GetRequiredService<T>();

    /// <summary>
    /// Registers and returns a mock for the specified service type.
    /// </summary>
    /// <typeparam name="T">The service type to mock.</typeparam>
    /// <param name="services">The service collection to register the mock in.</param>
    /// <param name="behavior">The mock behavior to use.</param>
    /// <returns>The configured mock instance.</returns>
    protected Mock<T> RegisterMock<T>(IServiceCollection services, MockBehavior behavior = MockBehavior.Default) where T : class
        => services.RegisterMock<T>(behavior);
}
