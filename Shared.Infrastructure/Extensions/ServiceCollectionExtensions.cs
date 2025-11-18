using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Extensions.Internal;
using Shared.Infrastructure.Modules;
using Shared.Infrastructure.Persistence.Migrations;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring the shared infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all shared infrastructure services to the service collection.
    /// This includes module registration, MediatR with behaviors, validators, and interceptors.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="modules">The list of enabled modules.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IList<IModule> modules)
    {
        // Register module information
        services.AddModuleInfo(modules);

        // Add MediatR with behaviors and handlers from all modules
        services.AddMediatRWithBehaviors(modules);

        // Add FluentValidation validators from all modules
        services.AddValidatorsFromModules(modules);

        // Add EF Core interceptors
        services.AddAuditableEntityInterceptor();
        services.AddDomainEventDispatchInterceptor();

        // Register services from each module
        services.RegisterModules(modules);

        return services;
    }

    /// <summary>
    /// Adds migration service for the specified DbContext.
    /// Runs on application startup to check for and apply pending migrations.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMigrationService<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        return services.AddHostedService<MigrationService<TContext>>();
    }
}
