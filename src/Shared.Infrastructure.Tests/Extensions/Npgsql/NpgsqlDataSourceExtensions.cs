using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Shared.Infrastructure.Tests.Extensions.Npgsql;

/// <summary>
/// Extension methods for test service configuration.
/// Provides utilities for replacing production services with test implementations.
/// </summary>
public static class NpgsqlDataSourceExtensions
{
    /// <summary>
    /// Replaces all registered NpgsqlDataSource instances with test versions using a test connection string.
    /// Handles both keyed and non-keyed service registrations.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="testConnectionString">The connection string to use for all data sources (from test container).</param>
    public static void ReplaceNpgsqlDataSources(this IServiceCollection services, string testConnectionString)
    {
        // Get all registered NpgsqlDataSource descriptors
        // Use .ToList() to avoid modifying collection while iterating
        var descriptors = services
            .Where(s => s.ServiceType == typeof(NpgsqlDataSource))
            .ToList();

        // Replace each registered data source
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);

            // Check if this is a keyed service
            if (descriptor.IsKeyedService)
            {
                // Register replacement with the same key
                services.AddKeyedSingleton(
                    descriptor.ServiceKey,
                    (_, _) => CreateDataSource(testConnectionString));
            }
            else
            {
                // Register non-keyed service
                services.AddSingleton(
                    _ => CreateDataSource(testConnectionString));
            }
        }
    }

    /// <summary>
    /// Creates a configured NpgsqlDataSource for testing.
    /// </summary>
    private static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        return builder.Build();
    }
}
