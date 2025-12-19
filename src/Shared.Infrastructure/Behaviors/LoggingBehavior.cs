using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Authorization;

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
/// <param name="user">The current user service.</param>
public class LoggingBehavior<TRequest>(ILogger<LoggingBehavior<TRequest>> logger, IUser user) : IRequestPreProcessor<TRequest>
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
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, user.Id, user.FullName, request);

        return Task.CompletedTask;
    }
}