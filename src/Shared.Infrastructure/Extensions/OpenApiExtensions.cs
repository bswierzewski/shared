using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring OpenAPI documentation.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds ProblemDetails and ValidationProblemDetails schemas to the OpenAPI document.
    /// This ensures frontend clients can properly type error responses from the API.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static OpenApiOptions AddProblemDetailsSchemas(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            // Initialize components if not already present
            document.Components ??= new OpenApiComponents();
            document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

            // Add ProblemDetails schema (RFC 7807)
            if (!document.Components.Schemas.ContainsKey("ProblemDetails"))
            {
                document.Components.Schemas["ProblemDetails"] = new OpenApiSchema
                {
                    Type = "object",
                    Description = "A machine-readable format for specifying errors in HTTP API responses based on RFC 7807.",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["type"] = new()
                        {
                            Type = "string",
                            Description = "A URI reference that identifies the problem type.",
                            Nullable = true,
                            Example = new Microsoft.OpenApi.Any.OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1")
                        },
                        ["title"] = new()
                        {
                            Type = "string",
                            Description = "A short, human-readable summary of the problem type.",
                            Nullable = true,
                            Example = new Microsoft.OpenApi.Any.OpenApiString("An error occurred while processing your request.")
                        },
                        ["status"] = new()
                        {
                            Type = "integer",
                            Format = "int32",
                            Description = "The HTTP status code.",
                            Nullable = true,
                            Example = new Microsoft.OpenApi.Any.OpenApiInteger(400)
                        },
                        ["detail"] = new()
                        {
                            Type = "string",
                            Description = "A human-readable explanation specific to this occurrence of the problem.",
                            Nullable = true
                        },
                        ["instance"] = new()
                        {
                            Type = "string",
                            Description = "A URI reference that identifies the specific occurrence of the problem.",
                            Nullable = true,
                            Example = new Microsoft.OpenApi.Any.OpenApiString("/api/snippets")
                        },
                        ["traceId"] = new()
                        {
                            Type = "string",
                            Description = "The trace identifier for correlating the error with logs.",
                            Nullable = true
                        }
                    },
                    AdditionalPropertiesAllowed = true
                };
            }

            // Add ValidationProblemDetails schema
            if (!document.Components.Schemas.ContainsKey("ValidationProblemDetails"))
            {
                document.Components.Schemas["ValidationProblemDetails"] = new OpenApiSchema
                {
                    Type = "object",
                    Description = "A ProblemDetails for validation errors, containing a dictionary of field-level errors.",
                    AllOf = new List<OpenApiSchema>
                    {
                        new() { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProblemDetails" } }
                    },
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["errors"] = new()
                        {
                            Type = "object",
                            Description = "Validation errors grouped by property name.",
                            AdditionalProperties = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema { Type = "string" }
                            },
                            Example = new Microsoft.OpenApi.Any.OpenApiObject
                            {
                                ["Title"] = new Microsoft.OpenApi.Any.OpenApiArray
                                {
                                    new Microsoft.OpenApi.Any.OpenApiString("Title is required.")
                                }
                            }
                        }
                    }
                };
            }

            return Task.CompletedTask;
        });

        return options;
    }
}
