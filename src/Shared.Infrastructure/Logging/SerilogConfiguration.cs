using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Shared.Infrastructure.Logging;

/// <summary>
/// Provides universal Serilog configuration for both Web and Console applications.
/// Automatically includes TraceId in all logs for request correlation and debugging.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog for ASP.NET Core Web applications with TraceId enrichment.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    /// <param name="applicationName">Optional application name for log enrichment.</param>
    /// <returns>The WebApplicationBuilder for method chaining.</returns>
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder,
        string? applicationName = null)
    {
        Log.Logger = CreateLogger(
            builder.Configuration,
            builder.Environment.EnvironmentName,
            applicationName ?? builder.Environment.ApplicationName);

        builder.Host.UseSerilog();
        return builder;
    }

    /// <summary>
    /// Configures Serilog for Console/Background applications with enhanced logging.
    /// </summary>
    /// <param name="builder">The HostBuilder instance.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="applicationName">Optional application name for log enrichment.</param>
    /// <returns>The HostBuilder for method chaining.</returns>
    public static IHostBuilder AddSerilog(
        this IHostBuilder builder,
        IConfiguration configuration,
        string? applicationName = null)
    {
        Log.Logger = CreateLogger(
            configuration,
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            applicationName ?? "ConsoleApp");

        builder.UseSerilog();
        return builder;
    }

    /// <summary>
    /// Adds Serilog request logging middleware with TraceId enrichment for Web applications.
    /// Must be called AFTER UseExceptionHandler and BEFORE other middleware.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    /// <returns>The WebApplication for method chaining.</returns>
    public static WebApplication UseSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value);
                diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

                // Add user information if authenticated
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
                }
            };
        });

        return app;
    }

    /// <summary>
    /// Creates a Serilog logger configuration with best practices for production use.
    /// </summary>
    private static Serilog.ILogger CreateLogger(
        IConfiguration configuration,
        string environmentName,
        string applicationName)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.WithMachineName();

        // Console output with TraceId
        loggerConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{TraceId}] {Message:lj}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Information);

        // File output for production debugging (rotating daily)
        var logPath = configuration["Logging:FilePath"] ?? "Logs/log-.txt";
        loggerConfiguration.WriteTo.File(
            path: logPath,
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: LogEventLevel.Warning,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{TraceId}] {Message:lj}{NewLine}{Exception}");

        return loggerConfiguration.CreateLogger();
    }
}
