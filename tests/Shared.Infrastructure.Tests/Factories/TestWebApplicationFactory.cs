using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Modules;
using Testcontainers.PostgreSql;

namespace Shared.Infrastructure.Tests.Factories;

/// <summary>
/// Test web application factory that manages PostgreSQL test containers and database setup for integration tests.
/// Provides isolated test databases with automatic cleanup and reset capabilities.
/// Generic version - subclasses can specify DbContext types specific to their domain.
/// </summary>
/// <typeparam name="TProgram">The Startup or Program type of the application being tested.</typeparam>
public abstract class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime, ITestWebApplicationFactory
    where TProgram : class
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithDatabase("postgres")
            .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    /// <summary>
    /// Tables that should not be reset when ResetDatabasesAsync is called.
    /// Override in subclasses to preserve system/reference data across tests.
    /// Example: return ["Roles", "Permissions"] to keep roles constant during test runs.
    /// </summary>
    protected virtual string[] TablesToIgnoreOnReset => [];

    /// <summary>
    /// Initializes the test factory by starting the PostgreSQL container and configuring the database respawner.
    /// Each module is responsible for its own migrations via the Initialize method in IModule.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();

        // Initialize all modules (runs migrations, seeds data, etc.)
        using (var scope = Services.CreateScope())
        {
            await scope.ServiceProvider.InitializeApplicationAsync();
        }

        // Combine default ignored tables with subclass-specific ones
        var tablesToIgnore = new[] { "__EFMigrationsHistory" }
            .Concat(TablesToIgnoreOnReset)
            .Select(t => new Respawn.Graph.Table(t))
            .ToArray();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = tablesToIgnore,
            WithReseed = true
        });
    }

    /// <summary>
    /// Disposes the test factory by closing the database connection and stopping the PostgreSQL container.
    /// </summary>
    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Resets all test databases to a clean state by truncating tables while preserving schema.
    /// </summary>
    public async Task ResetDatabasesAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    /// <summary>
    /// Configures the web host with test-specific settings including the test database connection and data protection.
    /// Automatically overrides all connection strings to use the test PostgreSQL container.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Override all connection strings with test container connection
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConnectionString = _container.GetConnectionString();

            // Get all existing connection strings from configuration
            var connectionStringsSection = context.Configuration.GetSection("ConnectionStrings");
            var connectionStringKeys = connectionStringsSection.GetChildren()
                .Select(x => x.Key)
                .ToArray();

            // Override all discovered connection strings with test container connection
            var overrides = new Dictionary<string, string?>();
            foreach (var key in connectionStringKeys)
            {
                overrides[$"ConnectionStrings:{key}"] = testConnectionString;
            }

            config.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            // Use ephemeral (in-memory) Data Protection keys for tests.
            // Keys are generated automatically but not persisted, avoiding file system operations
            // and preventing key ring errors while maintaining encryption functionality.
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            // Allow subclasses to configure additional services
            OnConfigureServices(services);
        });
    }

    /// <summary>
    /// Configures services for the test application.
    /// Override this method to configure additional services for the test application.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    protected virtual void OnConfigureServices(IServiceCollection services)
    {
        // Base implementation does nothing - subclasses override to add specific services
    }

    /// <summary>
    /// Implements ITestWebApplicationFactory.WithWebHostBuilder
    /// </summary>
    ITestWebApplicationFactory ITestWebApplicationFactory.WithWebHostBuilder(Action<IWebHostBuilder> configure)
    {
        return (ITestWebApplicationFactory)WithWebHostBuilder(configure);
    }
}
