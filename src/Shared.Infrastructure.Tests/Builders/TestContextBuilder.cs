using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
/// // Create container in fixture or test
/// var container = new PostgreSqlTestContainer();
/// await container.StartAsync();
/// 
/// // Build context using container
/// var context = await TestContext.CreateBuilder&lt;Program&gt;()
///     .WithContainer(container)
///     .WithServices((services, configuration) =&gt; { ... })
///     .BuildAsync();
/// </code>
/// </example>
public sealed class TestContextBuilder<TProgram> where TProgram : class
{
    private readonly List<Action<IServiceCollection, IConfiguration>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _hostConfigurations = new();
    private readonly DatabaseResetStrategy _resetStrategy = new();
    private string _environment = "Testing";
    private ITestContainer? _container;
    private bool _autoInitializeModules = true;

    internal TestContextBuilder() { }

    /// <summary>
    /// Configures the test to use a PostgreSQL container.
    /// The container lifecycle must be managed externally (start/stop in fixture or test).
    /// </summary>
    /// <param name="container">The container instance (must be started before BuildAsync).</param>
    /// <example>
    /// <code>
    /// // In fixture:
    /// public class MyFixture : IAsyncLifetime
    /// {
    ///     public PostgreSqlTestContainer Container { get; } = new();
    ///     public async Task InitializeAsync() =&gt; await Container.StartAsync();
    ///     public async Task DisposeAsync() =&gt; await Container.StopAsync();
    /// }
    /// 
    /// // In test:
    /// _context = await TestContext.CreateBuilder&lt;Program&gt;()
    ///     .WithContainer(fixture.Container)
    ///     .BuildAsync();
    /// </code>
    /// </example>
    public TestContextBuilder<TProgram> WithContainer(ITestContainer container)
    {
        ArgumentNullException.ThrowIfNull(container, nameof(container));
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
    /// Configures services for the test application with access to configuration.
    /// </summary>
    /// <param name="configure">Action to configure the service collection with access to IConfiguration.</param>
    /// <example>
    /// <code>
    /// .WithServices((services, configuration) =&gt;
    /// {
    ///     services.Configure&lt;MyOptions&gt;(configuration.GetSection("MySection"));
    /// })
    /// </code>
    /// </example>
    public TestContextBuilder<TProgram> WithServices(
        Action<IServiceCollection, IConfiguration> configure)
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
    /// Builds and initializes the test context.
    /// Container must be started before calling this method.
    /// </summary>
    /// <returns>A fully initialized test context ready for use.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no container is configured.</exception>
    public async Task<TestContext> BuildAsync()
    {
        if (_container == null)
        {
            throw new InvalidOperationException(
                "No container configured. Call WithContainer() before BuildAsync(). " +
                "Example: .WithContainer(fixture.Container)");
        }

        // Get connection string from container
        var connectionString = _container.GetConnectionString();

        // Build host
        var hostBuilder = new TestHostBuilder<TProgram>()
            .WithEnvironment(_environment)
            .WithContainer(_container);

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

        // Initialize modules (migrations, permissions sync, etc.)
        if (_autoInitializeModules)
        {
            var modules = host.Services.GetRequiredService<IEnumerable<IModule>>();
            await host.Services.InitializeModules(modules);
        }

        // Initialize database reset strategy (Respawn)
        await _resetStrategy.InitializeAsync(connectionString);

        // Set the reset strategy on the host for later use
        host.SetResetStrategy(_resetStrategy);

        // Reset database to clean state
        await host.ResetDatabaseAsync();

        return new TestContext(host);
    }
}
