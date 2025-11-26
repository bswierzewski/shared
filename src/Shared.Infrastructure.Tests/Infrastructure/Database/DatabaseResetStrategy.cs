using Npgsql;
using Respawn;

namespace Shared.Infrastructure.Tests.Infrastructure.Database;

/// <summary>
/// Encapsulates database reset configuration using Respawn.
/// Manages which tables to ignore during reset and handles Respawner lifecycle.
/// </summary>
public class DatabaseResetStrategy
{
    private readonly List<string> _tablesToIgnore = new() { "__EFMigrationsHistory" };
    private Respawner? _respawner;
    private NpgsqlConnection? _connection;

    /// <summary>
    /// Adds tables that should not be reset during database cleanup.
    /// Useful for preserving reference data like Roles, Permissions, etc.
    /// </summary>
    /// <param name="tables">Names of tables to ignore.</param>
    public void IgnoreTables(params string[] tables)
        => _tablesToIgnore.AddRange(tables);

    /// <summary>
    /// Initializes the Respawner with the specified connection string.
    /// Must be called after database migrations have been applied.
    /// </summary>
    /// <param name="connectionString">PostgreSQL connection string.</param>
    public async Task InitializeAsync(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = _tablesToIgnore
                .Select(t => new Respawn.Graph.Table(t))
                .ToArray(),
            WithReseed = true
        });
    }

    /// <summary>
    /// Resets the database to a clean state by truncating tables.
    /// Preserves schema and tables specified in IgnoreTables.
    /// </summary>
    public async Task ResetAsync()
    {
        if (_respawner != null && _connection != null)
        {
            await _respawner.ResetAsync(_connection);
        }
    }

    /// <summary>
    /// Disposes the database connection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
