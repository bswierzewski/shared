using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Persistence.Migrations;

/// <summary>
/// Generic migration service that runs database migrations for a specific DbContext.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MigrationService{TContext}"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider.</param>
public class MigrationService<TContext>(IServiceProvider serviceProvider)
    where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<MigrationService<TContext>> _logger = serviceProvider.GetRequiredService<ILogger<MigrationService<TContext>>>();

    /// <summary>
    /// Applies pending database migrations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            var contextName = typeof(TContext).Name;
            _logger.LogInformation("Checking migrations for {ContextName}...", contextName);

            // Check if database exists; if not, create it first
            var databaseExists = await context.Database.CanConnectAsync(cancellationToken);
            if (!databaseExists)
            {
                _logger.LogInformation("Database does not exist for {ContextName}, creating...", contextName);
                await context.Database.EnsureCreatedAsync(cancellationToken);
            }

            // Now safely get pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Found {Count} pending migrations for {ContextName}: [{Migrations}]",
                    pendingMigrations.Count(), contextName, string.Join(", ", pendingMigrations));
                await context.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("{ContextName} database migrations applied successfully", contextName);
            }
            else
            {
                _logger.LogInformation("No pending migrations for {ContextName} - database is up to date", contextName);
            }
        }
        catch (Exception ex)
        {
            var contextName = typeof(TContext).Name;
            _logger.LogError(ex, "Error occurred during {ContextName} database migration", contextName);
            throw;
        }
    }
}