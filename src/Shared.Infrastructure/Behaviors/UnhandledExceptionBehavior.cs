using MediatR;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that catches and handles unhandled exceptions in MediatR requests.
/// Provides centralized exception handling and logging for the entire application.
/// </summary>
/// <typeparam name="TRequest">The type of the MediatR request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// Initializes a new instance of the UnhandledExceptionBehavior class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
public class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Handles the request and catches any unhandled exceptions.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response or a failure result if an exception occurs.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "clean_architecture Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

            throw;
        }
    }
}