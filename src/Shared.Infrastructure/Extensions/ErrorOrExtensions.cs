using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for converting ErrorOr results to HTTP responses.
/// </summary>
public static class ErrorOrExtensions
{
    /// <summary>
    /// Converts an ErrorOr result to an IResult HTTP response with NoContent on success.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="errorOr">The ErrorOr result to convert.</param>
    /// <returns>An IResult representing the HTTP response.</returns>
    public static IResult ToNoContentResult<T>(this ErrorOr<T> errorOr)
    {
        return errorOr.Match(
            value => Results.NoContent(),
            errors => CreateProblemResult(errors));
    }

    /// <summary>
    /// Converts an ErrorOr result to an IResult HTTP response with Ok on success.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="errorOr">The ErrorOr result to convert.</param>
    /// <returns>An IResult representing the HTTP response.</returns>
    public static IResult ToHttpResult<T>(this ErrorOr<T> errorOr)
    {
        return errorOr.Match(
            value => Results.Ok(value),
            errors => CreateProblemResult(errors));
    }

    /// <summary>
    /// Converts an ErrorOr result to an IResult HTTP response with Created on success.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="errorOr">The ErrorOr result to convert.</param>
    /// <param name="uri">The URI of the created resource.</param>
    /// <returns>An IResult representing the HTTP response.</returns>
    public static IResult ToCreatedResult<T>(this ErrorOr<T> errorOr, string uri)
    {
        return errorOr.Match(
            value => Results.Created(uri, value),
            errors => CreateProblemResult(errors));
    }

    private static IResult CreateProblemResult(List<Error> errors)
    {
        var firstError = errors.FirstOrDefault();
        var statusCode = GetStatusCodeForErrorType(firstError.Type);

        if (firstError.Type == ErrorType.Validation)
        {
            return Results.ValidationProblem(
                errors.ToDictionary(),
                detail: firstError.Description,
                title: GetTitleForStatusCode(statusCode),
                type: GetTypeForStatusCode(statusCode),
                statusCode: statusCode);
        }

        return Results.Problem(
            detail: firstError.Description,
            statusCode: statusCode,
            title: GetTitleForStatusCode(statusCode),
            type: GetTypeForStatusCode(statusCode));
    }

    private static int GetStatusCodeForErrorType(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status400BadRequest,
        ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        _ => "An error occurred"
    };

    private static string GetTypeForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    private static IDictionary<string, string[]> ToDictionary(this List<Error> errors)
    {
        return errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray()
            );
    }
}
