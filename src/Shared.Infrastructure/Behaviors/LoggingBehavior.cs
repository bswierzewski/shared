using MediatR;
using Shared.Abstractions.Authorization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that logs information about MediatR requests and responses.
/// Provides detailed logging for monitoring and debugging purposes.
/// </summary>
/// <typeparam name="TRequest">The type of the MediatR request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// Initializes a new instance of the LoggingBehavior class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
/// <param name="user">The current user service.</param>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IUser user) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;
    private readonly IUser _user = user;

    /// <summary>
    /// Handles the request with comprehensive logging.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the next behavior in the pipeline.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        // Log request start
        _logger.LogInformation(
            "Starting request {RequestName} with ID {RequestId} for user {UserId}",
            requestName,
            requestId,
            _user.IsAuthenticated && _user.Id.HasValue ? _user.Id.Value.ToString() : "Unknown");

        // Log request details in debug mode
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Request {RequestName} ({RequestId}) details: {@Request}",
                requestName,
                requestId,
                request);
        }

        try
        {
            var response = await next();

            stopwatch.Stop();

            // Log successful completion
            _logger.LogInformation(
                "Completed request {RequestName} with ID {RequestId} in {ElapsedMilliseconds}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            // Log response details in debug mode
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Request {RequestName} ({RequestId}) response: {@Response}",
                    requestName,
                    requestId,
                    response);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log request failure
            _logger.LogError(ex,
                "Request {RequestName} with ID {RequestId} failed after {ElapsedMilliseconds}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}