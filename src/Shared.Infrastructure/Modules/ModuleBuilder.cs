using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Npgsql;
using Shared.Infrastructure.Behaviors;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Shared.Infrastructure.Modules;

/// <summary>
/// Builder for configuring module services and options.
/// Provides a fluent API for registering module dependencies.
/// </summary>
public class ModuleBuilder(IServiceCollection services, IConfiguration configuration, string moduleName)
{
    /// <summary>
    /// Gets the service collection for registering services.
    /// </summary>
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    public IConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Gets the name of the module being built.
    /// </summary>
    public string ModuleName { get; } = moduleName ?? throw new ArgumentNullException(nameof(moduleName));

    /// <summary>
    /// Configures module-specific options using the provided configuration action.
    /// </summary>
    /// <param name="configureOptions">An action that receives the service collection and configuration to configure module options.</param>
    /// <returns>The current module builder instance for method chaining.</returns>
    public ModuleBuilder AddOptions(Action<IServiceCollection, IConfiguration> configureOptions)
    {
        configureOptions(Services, Configuration);

        return this;
    }

    /// <summary>
    /// Registers PostgreSQL database context and persistence infrastructure.
    /// Configures Npgsql data source, Entity Framework Core with interceptors, and repository interface.
    /// </summary>
    /// <typeparam name="TDbContext">The Entity Framework DbContext type to register.</typeparam>
    /// <typeparam name="TInterface">The database context interface to register.</typeparam>
    /// <param name="connectionStringFactory">A factory function that produces the PostgreSQL connection string.</param>
    /// <returns>The current module builder instance for method chaining.</returns>
    public ModuleBuilder AddPostgres<TDbContext, TInterface>(
        Func<IServiceProvider, string> connectionStringFactory)
        where TDbContext : DbContext, TInterface
        where TInterface : class
    {
        Services.AddKeyedSingleton(ModuleName, (sp, key) =>
        {
            var connectionString = connectionStringFactory(sp);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string for module '{ModuleName}' is empty.");

            return NpgsqlDataSource.Create(connectionString);
        });
        
        Services.AddScoped<SaveChangesInterceptor, AuditableEntityInterceptor>();
        Services.AddScoped<SaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        Services.AddDbContext<TDbContext>((sp, options) =>
        {
            var dataSource = sp.GetRequiredKeyedService<NpgsqlDataSource>(ModuleName);

            options.UseNpgsql(dataSource)
                   .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        Services.AddScoped<TInterface>(sp => sp.GetRequiredService<TDbContext>());

        return this;
    }

    /// <summary>
    /// Registers MediatR CQRS infrastructure with pipeline behaviors and validators.
    /// Configures handlers, validators, and pipeline behaviors for the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for MediatR handlers and validators.</param>
    /// <returns>The current module builder instance for method chaining.</returns>
    public ModuleBuilder AddCQRS(params Assembly[] assemblies)
    {
        Services.AddMediatR(config =>
        {
            foreach (var assembly in assemblies)
                config.RegisterServicesFromAssembly(assembly);
                
            config.AddOpenRequestPreProcessor(typeof(LoggingBehavior<>));
            config.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        foreach (var assembly in assemblies)
            Services.AddValidatorsFromAssembly(assembly);

        return this;
    }

    /// <summary>
    /// Builds and returns the configured service collection with all module services.
    /// </summary>
    /// <returns>The service collection with all registered module services.</returns>
    public IServiceCollection Build() => Services;
}

/// <summary>
/// Extension methods for creating ModuleBuilder instances.
/// </summary>
public static class ModuleBuilderExtensions
{
    /// <summary>
    /// Creates a ModuleBuilder for fluent configuration of module services and dependencies.
    /// </summary>
    /// <param name="services">The service collection to register module services into.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="moduleName">The name of the module being configured.</param>
    /// <returns>A ModuleBuilder instance for fluent API configuration.</returns>
    public static ModuleBuilder AddModule(this IServiceCollection services, IConfiguration configuration, string moduleName)
    {
        return new ModuleBuilder(services, configuration, moduleName);
    }
}
