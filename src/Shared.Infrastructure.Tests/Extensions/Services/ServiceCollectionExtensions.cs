using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Shared.Infrastructure.Tests.Extensions.Services;

/// <summary>
/// Extension methods for IServiceCollection to support test-specific service registration patterns.
/// Provides discoverable, intuitive API for common test scenarios.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replaces a service with a mock implementation.
    /// Returns the mock for further configuration.
    /// </summary>
    /// <typeparam name="TService">The service type to mock.</typeparam>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="behavior">The mock behavior to use (default is MockBehavior.Default).</param>
    /// <returns>The configured mock instance for further setup.</returns>
    /// <example>
    /// <code>
    /// var emailMock = services.ReplaceMock&lt;IEmailService&gt;();
    /// emailMock.Setup(x => x.SendAsync(It.IsAny&lt;Email&gt;()))
    ///     .ReturnsAsync(true);
    /// </code>
    /// </example>
    public static Mock<TService> ReplaceMock<TService>(
        this IServiceCollection services,
        MockBehavior behavior = MockBehavior.Default)
        where TService : class
    {
        var mock = new Mock<TService>(behavior);
        services.RemoveAll<TService>();
        services.AddSingleton(mock.Object);
        return mock;
    }

    /// <summary>
    /// Replaces a service with a test implementation.
    /// </summary>
    /// <typeparam name="TService">The service interface type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="lifetime">The service lifetime (default is Singleton).</param>
    /// <returns>The modified service collection for method chaining.</returns>
    /// <example>
    /// <code>
    /// services.ReplaceService&lt;IEmailService, FakeEmailService&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection ReplaceService<TService, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService : class
        where TImplementation : class, TService
    {
        services.RemoveAll<TService>();

        return lifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton<TService, TImplementation>(),
            ServiceLifetime.Scoped => services.AddScoped<TService, TImplementation>(),
            ServiceLifetime.Transient => services.AddTransient<TService, TImplementation>(),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime))
        };
    }

    /// <summary>
    /// Replaces a service with a specific instance.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="instance">The instance to register.</param>
    /// <returns>The modified service collection for method chaining.</returns>
    /// <example>
    /// <code>
    /// var fakeConfig = new FakeConfiguration();
    /// services.ReplaceInstance&lt;IConfiguration&gt;(fakeConfig);
    /// </code>
    /// </example>
    public static IServiceCollection ReplaceInstance<TService>(
        this IServiceCollection services,
        TService instance)
        where TService : class
    {
        services.RemoveAll<TService>();
        services.AddSingleton(instance);
        return services;
    }
}
