using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that logs information about MediatR requests and responses.
/// Provides detailed logging for monitoring and debugging purposes.
/// </summary>
/// <typeparam name="TRequest">The type of the MediatR request.</typeparam>
/// <remarks>
/// Initializes a new instance of the LoggingBehavior class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
public class LoggingBehavior<TRequest>(ILogger<LoggingBehavior<TRequest>> logger) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    /// <summary>
    /// Processes the request by logging relevant information.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling: {RequestName}: {@Payload}", typeof(TRequest).Name, request);

        return Task.CompletedTask;
    }
}