using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Tests.Core;
using Shared.Infrastructure.Tests.Infrastructure.Containers;

namespace Shared.Infrastructure.Tests.Builders;

/// <summary>
/// Internal builder for configuring and creating test hosts.
/// Used by TestContextBuilder to encapsulate WebApplicationFactory configuration.
/// </summary>
/// <typeparam name="TProgram">The Program or Startup class of the application under test.</typeparam>
internal class TestHostBuilder<TProgram> where TProgram : class
{
    private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _hostConfigurations = new();
    private readonly List<string> _connectionStringConfigKeys = new() { "ConnectionStrings:DefaultConnection" };
    private ITestContainer? _container;
    private string _environment = "Testing";

    /// <summary>
    /// Sets the test container to use for the test host.
    /// </summary>
    public TestHostBuilder<TProgram> WithContainer(ITestContainer? container)
    {
        _container = container;
        return this;
    }

    /// <summary>
    /// Sets the hosting environment.
    /// </summary>
    public TestHostBuilder<TProgram> WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    /// <summary>
    /// Adds a service configuration action.
    /// </summary>
    public TestHostBuilder<TProgram> WithServices(Action<IServiceCollection> configure)
    {
        _serviceConfigurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Adds a web host configuration action.
    /// </summary>
    public TestHostBuilder<TProgram> WithHostConfiguration(Action<IWebHostBuilder> configure)
    {
        _hostConfigurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Adds configuration keys for connection strings that should be overridden with the test container connection string.
    /// Use this to specify module-specific connection string keys (e.g., "Modules__Users:ConnectionString").
    /// </summary>
    /// <param name="configKeys">The configuration keys to override.</param>
    public TestHostBuilder<TProgram> WithConnectionStringKeys(params string[] configKeys)
    {
        _connectionStringConfigKeys.AddRange(configKeys);
        return this;
    }

    /// <summary>
    /// Builds the test host with all configured settings.
    /// </summary>
    public TestHost<TProgram> Build()
    {
        return new TestHost<TProgram>(
            _container,
            _serviceConfigurations,
            _hostConfigurations,
            _connectionStringConfigKeys,
            _environment);
    }
}
