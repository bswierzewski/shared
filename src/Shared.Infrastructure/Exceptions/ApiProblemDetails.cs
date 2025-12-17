using Microsoft.AspNetCore.Mvc;

namespace Shared.Infrastructure.Exceptions;

/// <summary>
/// API problem details response that includes traceId for request tracking and optional validation errors.
/// </summary>
public class ApiProblemDetails : ProblemDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiProblemDetails"/> class.
    /// </summary>
    public ApiProblemDetails()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiProblemDetails"/> class with validation errors.
    /// </summary>
    /// <param name="errors">The validation errors grouped by field name.</param>
    public ApiProblemDetails(IDictionary<string, string[]> errors)
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets or sets the trace identifier for request tracking and debugging.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the validation errors grouped by field name. Null if no validation errors.
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; set; }
}
