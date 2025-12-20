using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Shared.Infrastructure.Logging;

/// <summary>
/// Provides simplified Serilog configuration using appsettings.json.
/// TraceId enrichment is configured via Serilog.Enrichers.Span in appsettings.json.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog using appsettings.json.
    /// All configuration (sinks, templates, levels, enrichers) should be in appsettings.json.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    /// <returns>The WebApplicationBuilder for method chaining.</returns>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);
        });

        return builder;
    }

    /// <summary>
    /// Adds request logging middleware with User enrichment.
    /// Must be called AFTER UseExceptionHandler and BEFORE other middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("User", httpContext.User.Identity.Name);
                }
            };
        });
    }
}
