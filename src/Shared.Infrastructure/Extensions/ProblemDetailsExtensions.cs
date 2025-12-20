using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring ProblemDetails with custom behavior.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Adds custom configuration to ProblemDetails options, including timestamp and development-only exception details.
    /// </summary>
    /// <param name="options">The ProblemDetails options to configure.</param>
    /// <param name="environment">The host environment to determine if running in development.</param>
    /// <returns>The ProblemDetails options for chaining.</returns>
    public static ProblemDetailsOptions AddCustomConfiguration(
        this ProblemDetailsOptions options,
        IHostEnvironment environment)
    {
        options.CustomizeProblemDetails = context =>
        {
            // Add timestamp to all Problem Details
            context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            // In Development: Add exception details for debugging
            if (environment.IsDevelopment() && context.Exception is not null)
            {
                context.ProblemDetails.Extensions["exceptionType"] = context.Exception.GetType().Name;
                context.ProblemDetails.Extensions["exceptionMessage"] = context.Exception.Message;
                context.ProblemDetails.Extensions["stackTrace"] = context.Exception.StackTrace;
            }
        };

        return options;
    }
}
