using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Exceptions;

/// <summary>
/// Global exception handler that converts all exceptions into appropriate HTTP responses.
/// Implements best practices for security and observability.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
/// </remarks>
/// <param name="logger">The logger instance for logging unhandled exceptions.</param>
/// <param name="environment">The host environment to determine if running in development.</param>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the specified exception.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Always returns true as all exceptions are handled.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Log the unexpected exception with full details
        logger.LogError(
            exception,
            "An unhandled exception occurred while processing request {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        // In Production: NEVER expose exception details
        // In Development: Show details for debugging
        var detail = environment.IsDevelopment()
            ? exception.ToString()
            : "Wystąpił nieoczekiwany błąd serwera. Prosimy spróbować później.";

        var result = Results.Problem(
            detail: detail,
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An error occurred while processing your request.",
            type: "https://tools.ietf.org/html/rfc7231#section-6.6.1");

        await result.ExecuteAsync(httpContext);

        return true;
    }
}
