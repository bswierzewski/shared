using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Exceptions;

/// <summary>
/// Global exception handler that converts all exceptions into appropriate HTTP responses.
/// Implements best practices for security and observability.
/// </summary>
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;
    private readonly ILogger<ApiExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging unhandled exceptions.</param>
    /// <param name="environment">The host environment to determine if running in development.</param>
    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;

        // Register known exception types and handlers.
        _exceptionHandlers = new()
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
        };
    }

    /// <summary>
    /// Attempts to handle the specified exception.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Always returns true as all exceptions are handled.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        // Handle known exceptions
        if (_exceptionHandlers.TryGetValue(exceptionType, out var handler))
        {
            await handler.Invoke(httpContext, exception);
            return true;
        }

        // Handle all other (unexpected) exceptions
        await HandleUnknownException(httpContext, exception, cancellationToken);
        return true;
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ApiProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = httpContext.Request.Path,
            TraceId = httpContext.TraceIdentifier
        });
    }

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new ApiProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = exception.Message, // Safe - you control this message
            Instance = httpContext.Request.Path,
            TraceId = httpContext.TraceIdentifier
        });
    }

    private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(new ApiProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Instance = httpContext.Request.Path,
            TraceId = httpContext.TraceIdentifier
        });
    }

    private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ApiProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Instance = httpContext.Request.Path,
            TraceId = httpContext.TraceIdentifier
        });
    }

    private async Task HandleUnknownException(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the unexpected exception with full details
        _logger.LogError(
            exception,
            "An unhandled exception occurred while processing request {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // In Production: NEVER expose exception details
        // In Development: Show details for debugging
        var problemDetails = new ApiProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Instance = httpContext.Request.Path,
            TraceId = httpContext.TraceIdentifier
        };

        // Only include exception details in Development
        if (_environment.IsDevelopment())
        {
            problemDetails.Detail = exception.ToString();
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }
}
