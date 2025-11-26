using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Tests.Infrastructure.Containers;
using Shared.Infrastructure.Tests.Infrastructure.Database;

namespace Shared.Infrastructure.Tests.Core;

/// <summary>
/// Default implementation of test host using WebApplicationFactory.
/// Manages the test application lifecycle, configuration overrides, and database management.
/// </summary>
/// <typeparam name="TProgram">The Program or Startup class of the application under test.</typeparam>
internal class TestHost<TProgram> : WebApplicationFactory<TProgram>, ITestHost where TProgram : class
{
    private readonly ITestContainer? _container;
    private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _hostConfigurations = new();
    private readonly string[] _connectionStringConfigKeys;
    private DatabaseResetStrategy? _resetStrategy;
    private string _environment = "Testing";

    public TestHost(
        ITestContainer? container,
        IEnumerable<Action<IServiceCollection>> serviceConfigurations,
        IEnumerable<Action<IWebHostBuilder>> hostConfigurations,
        IEnumerable<string> connectionStringConfigKeys,
        string environment)
    {
        _container = container;
        _serviceConfigurations.AddRange(serviceConfigurations);
        _hostConfigurations.AddRange(hostConfigurations);
        _connectionStringConfigKeys = connectionStringConfigKeys.ToArray();
        _environment = environment;
    }

    /// <summary>
    /// Sets the database reset strategy. Called by TestContextBuilder after initialization.
    /// </summary>
    internal void SetResetStrategy(DatabaseResetStrategy strategy)
    {
        _resetStrategy = strategy;
    }

    /// <summary>
    /// Resets the database using the configured reset strategy.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_resetStrategy != null)
        {
            await _resetStrategy.ResetAsync();
        }
    }

    /// <summary>
    /// Configures the web host with test-specific settings.
    /// Overrides connection strings, sets environment, and applies custom configurations.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);

        // Override connection strings if container is configured
        if (_container != null)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var testConnectionString = _container.GetConnectionString();

                // Override all configured connection string keys with test container connection
                var overrides = new Dictionary<string, string?>();
                foreach (var configKey in _connectionStringConfigKeys)
                {
                    overrides[configKey] = testConnectionString;
                }

                config.AddInMemoryCollection(overrides);
            });
        }

        // Apply custom host configurations
        foreach (var configure in _hostConfigurations)
        {
            configure(builder);
        }

        builder.ConfigureServices(services =>
        {
            // Use ephemeral Data Protection keys for tests
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            // Register database manager for reset operations
            services.AddSingleton<IDatabaseManager, DatabaseManager>();

            // Apply custom service configurations
            foreach (var configure in _serviceConfigurations)
            {
                configure(services);
            }
        });
    }

    /// <summary>
    /// Disposes the test host and associated resources.
    /// </summary>
    public new async ValueTask DisposeAsync()
    {
        if (_resetStrategy != null)
        {
            await _resetStrategy.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.StopAsync();
        }

        await base.DisposeAsync();
    }
}
