using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Shared.Infrastructure.Modules.Internal;

/// <summary>
/// Internal extension methods for configuring persistence-related services.
/// </summary>
internal static class PersistenceExtensions
{
    /// <summary>
    /// Adds the auditable entity interceptor.
    /// Automatically populates CreatedAt, CreatedBy, ModifiedAt, ModifiedBy fields.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection AddAuditableEntityInterceptor(this IServiceCollection services)
    {
        return services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
    }

    /// <summary>
    /// Adds the domain event dispatch interceptor.
    /// Automatically publishes domain events via MediatR after SaveChanges.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection AddDomainEventDispatchInterceptor(this IServiceCollection services)
    {
        return services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
    }
}
