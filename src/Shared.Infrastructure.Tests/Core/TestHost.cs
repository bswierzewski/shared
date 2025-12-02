using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Tests.Infrastructure.Containers;
using Shared.Infrastructure.Tests.Infrastructure.Database;
using Shared.Infrastructure.Tests.Extensions.Npgsql;

namespace Shared.Infrastructure.Tests.Core;

/// <summary>
/// Default implementation of test host using WebApplicationFactory.
/// Manages the test application lifecycle, configuration overrides, and database management.
/// Container lifecycle is managed externally by the fixture or test.
/// </summary>
/// <typeparam name="TProgram">The Program or Startup class of the application under test.</typeparam>
internal class TestHost<TProgram> : WebApplicationFactory<TProgram>, ITestHost where TProgram : class
{
    private readonly ITestContainer? _container;
    private readonly List<Action<IServiceCollection, IConfiguration>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _hostConfigurations = new();
    private DatabaseResetStrategy? _resetStrategy;
    private string _environment = "Testing";

    public TestHost(
        ITestContainer? container,
        IEnumerable<Action<IServiceCollection, IConfiguration>> serviceConfigurations,
        IEnumerable<Action<IWebHostBuilder>> hostConfigurations,
        string environment)
    {
        _container = container;
        _serviceConfigurations.AddRange(serviceConfigurations);
        _hostConfigurations.AddRange(hostConfigurations);
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

        // Configure logging to output to console during tests
        builder.ConfigureLogging((context, logging) =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        // Apply custom host configurations
        foreach (var configure in _hostConfigurations)
        {
            configure(builder);
        }

        builder.ConfigureServices((context, services) =>
        {
            // Replace all NpgsqlDataSource instances with test container's connection
            if (_container != null)
            {
                services.ReplaceNpgsqlDataSources(_container.GetConnectionString());
            }

            // Use ephemeral Data Protection keys for tests
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            // Register database manager for reset operations
            services.AddSingleton<IDatabaseManager, DatabaseManager>();

            // Apply custom service configurations with access to configuration
            foreach (var configure in _serviceConfigurations)
            {
                configure(services, context.Configuration);
            }
        });
    }

    /// <summary>
    /// Disposes the test host and associated resources.
    /// Container is NOT stopped here - it's managed by the fixture or test.
    /// </summary>
    public new async ValueTask DisposeAsync()
    {
        if (_resetStrategy != null)
        {
            await _resetStrategy.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
