using MediatR;
using Shared.Abstractions.Authorization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that monitors the performance of MediatR requests.
/// Logs warnings for requests that take longer than specified thresholds.
/// </summary>
/// <typeparam name="TRequest">The type of the MediatR request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// Initializes a new instance of the PerformanceBehavior class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
/// <param name="user">The current user service.</param>
public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, IUser user) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger = logger;
    private readonly IUser _user = user;
    private readonly Stopwatch _timer = new Stopwatch();

    /// <summary>
    /// The threshold in milliseconds for logging performance warnings.
    /// Requests taking longer than this will be logged as warnings.
    /// </summary>
    public const int PerformanceThresholdMs = 500;

    /// <summary>
    /// The threshold in milliseconds for logging critical performance issues.
    /// Requests taking longer than this will be logged as errors.
    /// </summary>
    public const int CriticalPerformanceThresholdMs = 2000;

    /// <summary>
    /// Handles the request with performance monitoring.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the next behavior in the pipeline.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;
        var requestName = typeof(TRequest).Name;

        // Log performance metrics based on elapsed time
        if (elapsedMilliseconds > CriticalPerformanceThresholdMs)
        {
            _logger.LogError(
                "CRITICAL PERFORMANCE: Request {RequestName} took {ElapsedMilliseconds}ms to complete. " +
                "User: {UserId}. Request: {@Request}",
                requestName,
                elapsedMilliseconds,
                _user.IsAuthenticated && _user.Id.HasValue ? _user.Id.Value.ToString() : "Unknown",
                request);
        }
        else if (elapsedMilliseconds > PerformanceThresholdMs)
        {
            _logger.LogWarning(
                "SLOW REQUEST: Request {RequestName} took {ElapsedMilliseconds}ms to complete. " +
                "User: {UserId}",
                requestName,
                elapsedMilliseconds,
                _user.IsAuthenticated && _user.Id.HasValue ? _user.Id.Value.ToString() : "Unknown");

            // Log request details only in debug mode for performance warnings
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Slow request {RequestName} details: {@Request}",
                    requestName,
                    request);
            }
        }
        else if (_logger.IsEnabled(LogLevel.Debug))
        {
            // Log fast requests only in debug mode
            _logger.LogDebug(
                "Request {RequestName} completed in {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);
        }

        // Log performance metrics for structured logging and monitoring
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestName"] = requestName,
            ["ElapsedMilliseconds"] = elapsedMilliseconds,
            ["UserId"] = _user.IsAuthenticated && _user.Id.HasValue ? _user.Id.Value : (object)"Unknown",
            ["IsSlowRequest"] = elapsedMilliseconds > PerformanceThresholdMs,
            ["IsCriticalPerformance"] = elapsedMilliseconds > CriticalPerformanceThresholdMs
        }))
        {
            _logger.LogInformation(
                "Performance metrics for {RequestName}: {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);
        }

        return response;
    }
}