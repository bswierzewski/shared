using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Modules;
using Shared.Infrastructure.Tests.Core;
using Shared.Infrastructure.Tests.Infrastructure.Containers;
using Shared.Infrastructure.Tests.Infrastructure.Database;

namespace Shared.Infrastructure.Tests.Builders;

/// <summary>
/// Fluent builder for configuring and creating test contexts.
/// Provides explicit, readable test setup with clear initialization flow.
/// </summary>
/// <typeparam name="TProgram">The Program or Startup class of the application under test.</typeparam>
/// <example>
/// <code>
/// var context = await TestContext.CreateBuilder&lt;Program&gt;()
///     .WithPostgreSql()
///     .WithTablesIgnoredOnReset("Roles", "Permissions")
///     .WithServices(services =&gt;
///     {
///         services.ReplaceMock&lt;IEmailService&gt;();
///     })
///     .BuildAsync();
/// </code>
/// </example>
public sealed class TestContextBuilder<TProgram> where TProgram : class
{
    private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _hostConfigurations = new();
    private readonly List<string> _connectionStringConfigKeys = new();
    private readonly DatabaseResetStrategy _resetStrategy = new();
    private string _environment = "Testing";
    private ITestContainer? _container;
    private bool _autoInitializeModules = true;

    internal TestContextBuilder() { }

    /// <summary>
    /// Configures the test to use a PostgreSQL test container.
    /// The container will be started during BuildAsync().
    /// </summary>
    /// <param name="configure">Optional configuration for the PostgreSQL container.</param>
    public TestContextBuilder<TProgram> WithPostgreSql(
        Action<PostgreSqlTestContainer>? configure = null)
    {
        var container = new PostgreSqlTestContainer();
        configure?.Invoke(container);
        _container = container;
        return this;
    }

    /// <summary>
    /// Specifies tables that should not be reset between tests.
    /// Useful for preserving reference data like Roles or Permissions.
    /// </summary>
    /// <param name="tables">Names of tables to ignore during database reset.</param>
    public TestContextBuilder<TProgram> WithTablesIgnoredOnReset(params string[] tables)
    {
        _resetStrategy.IgnoreTables(tables);
        return this;
    }

    /// <summary>
    /// Configures services for the test application.
    /// Use extension methods like ReplaceMock&lt;T&gt;() for common scenarios.
    /// </summary>
    /// <param name="configure">Action to configure the service collection.</param>
    /// <example>
    /// <code>
    /// .WithServices(services =>
    /// {
    ///     var mock = services.ReplaceMock&lt;IEmailService&gt;();
    ///     mock.Setup(x => x.SendAsync(It.IsAny&lt;Email&gt;())).ReturnsAsync(true);
    /// })
    /// </code>
    /// </example>
    public TestContextBuilder<TProgram> WithServices(
        Action<IServiceCollection> configure)
    {
        _serviceConfigurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Configures the web host builder.
    /// Use this for advanced scenarios that require direct access to IWebHostBuilder.
    /// </summary>
    /// <param name="configure">Action to configure the web host builder.</param>
    public TestContextBuilder<TProgram> WithHostConfiguration(
        Action<IWebHostBuilder> configure)
    {
        _hostConfigurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Sets the hosting environment (default: "Testing").
    /// </summary>
    /// <param name="environment">The environment name (e.g., "Development", "Testing", "Production").</param>
    public TestContextBuilder<TProgram> WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    /// <summary>
    /// Disables automatic module initialization.
    /// By default, modules are initialized (migrations, permission sync, etc.) after the host is built.
    /// Use this if you need to manually control module initialization.
    /// </summary>
    public TestContextBuilder<TProgram> WithoutModuleInitialization()
    {
        _autoInitializeModules = false;
        return this;
    }

    /// <summary>
    /// Adds configuration keys for connection strings that should be overridden with the test container connection string.
    /// Use this to specify module-specific connection string keys.
    /// </summary>
    /// <param name="configKeys">The configuration keys to override (e.g., "Modules__Users:ConnectionString").</param>
    /// <example>
    /// <code>
    /// .WithConnectionStringKeys($"{UsersDbContextOptions.SectionName}:ConnectionString")
    /// </code>
    /// </example>
    public TestContextBuilder<TProgram> WithConnectionStringKeys(params string[] configKeys)
    {
        _connectionStringConfigKeys.AddRange(configKeys);
        return this;
    }

    /// <summary>
    /// Builds and initializes the test context.
    /// This performs the following steps in order:
    /// <list type="number">
    /// <item>Start the test container (if configured)</item>
    /// <item>Build the test host</item>
    /// <item>Initialize modules (migrations, permissions sync, etc.)</item>
    /// <item>Initialize database reset strategy (Respawn)</item>
    /// <item>Reset database to clean state</item>
    /// </list>
    /// </summary>
    /// <returns>A fully initialized test context ready for use.</returns>
    public async Task<TestContext> BuildAsync()
    {
        // Step 1: Start container
        if (_container != null)
        {
            await _container.StartAsync();
        }

        // Step 2: Build host
        var hostBuilder = new TestHostBuilder<TProgram>()
            .WithEnvironment(_environment)
            .WithContainer(_container);

        // Apply connection string keys
        if (_connectionStringConfigKeys.Any())
        {
            hostBuilder.WithConnectionStringKeys(_connectionStringConfigKeys.ToArray());
        }

        // Apply all host configurations
        foreach (var configure in _hostConfigurations)
        {
            hostBuilder.WithHostConfiguration(configure);
        }

        // Apply all service configurations
        foreach (var configure in _serviceConfigurations)
        {
            hostBuilder.WithServices(configure);
        }

        var host = hostBuilder.Build();

        // Step 3: Initialize modules (if enabled)
        // This runs migrations, syncs permissions, etc.
        if (_autoInitializeModules)
        {
            var modules = host.Services.GetRequiredService<IEnumerable<IModule>>();
            await host.Services.InitializeModules(modules);
        }

        // Step 4: Initialize database reset strategy
        // Respawner needs the database schema to exist, so this must happen after migrations
        if (_container != null)
        {
            await _resetStrategy.InitializeAsync(_container.GetConnectionString());
        }

        // Set the reset strategy on the host for later use
        host.SetResetStrategy(_resetStrategy);

        // Step 5: Reset database to clean state
        await host.ResetDatabaseAsync();

        return new TestContext(host);
    }
}
