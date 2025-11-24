using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Shared.Infrastructure.Tests.Extensions;

/// <summary>
/// Extensions for IServiceCollection to support test-specific service registration patterns.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a mock instance of a service, replacing any existing registration.
    /// </summary>
    /// <typeparam name="TService">The service type to mock.</typeparam>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="behavior">The mock behavior to use (default is MockBehavior.Default).</param>
    /// <returns>The configured mock instance for further setup.</returns>
    public static Mock<TService> RegisterMock<TService>(this IServiceCollection services, MockBehavior behavior = MockBehavior.Default) where TService : class
    {
        var mock = new Mock<TService>(behavior);
        services.RemoveAll<TService>();
        services.AddSingleton(mock.Object);
        return mock;
    }

    /// <summary>
    /// Registers a service implementation, replacing any existing registration with singleton lifetime.
    /// </summary>
    /// <typeparam name="TService">The service interface type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="services">The service collection to modify.</param>
    /// <returns>The modified service collection for method chaining.</returns>
    public static IServiceCollection RegisterService<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.RemoveAll<TService>();
        services.AddSingleton<TService, TImplementation>();
        return services;
    }
}
