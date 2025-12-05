using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring OpenAPI documentation.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds ProblemDetails and ValidationProblemDetails schemas to the OpenAPI document.
    /// Uses direct assignment to avoid conflicts with built-in transformers.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static OpenApiOptions AddProblemDetailsSchemas(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

            var schemas = document.Components.Schemas;

            // Use indexer assignment instead of TryAdd - overwrites if exists, adds if not
            schemas["ProblemDetails"] = new OpenApiSchema
            {
                Type = "object",
                Description = "A machine-readable format for specifying errors in HTTP API responses based on RFC 7807.",
                AdditionalPropertiesAllowed = true,
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["type"] = new()
                    {
                        Type = "string",
                        Nullable = true,
                        Description = "A URI reference that identifies the problem type.",
                        Example = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.1")
                    },
                    ["title"] = new()
                    {
                        Type = "string",
                        Nullable = true,
                        Description = "A short, human-readable summary of the problem type.",
                        Example = new OpenApiString("Bad Request")
                    },
                    ["status"] = new()
                    {
                        Type = "integer",
                        Format = "int32",
                        Nullable = true,
                        Description = "The HTTP status code.",
                        Example = new OpenApiInteger(400)
                    },
                    ["detail"] = new()
                    {
                        Type = "string",
                        Nullable = true,
                        Description = "A human-readable explanation specific to this occurrence of the problem."
                    },
                    ["instance"] = new()
                    {
                        Type = "string",
                        Nullable = true,
                        Description = "A URI reference that identifies the specific occurrence of the problem."
                    },
                    ["traceId"] = new()
                    {
                        Type = "string",
                        Nullable = true,
                        Description = "The trace identifier for correlating the error with logs."
                    }
                }
            };

            schemas["ValidationProblemDetails"] = new OpenApiSchema
            {
                Type = "object",
                Description = "A ProblemDetails for validation errors.",
                AllOf =
                [
                    new() { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProblemDetails" } }
                ],
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
                        Example = new OpenApiObject
                        {
                            ["Title"] = new OpenApiArray { new OpenApiString("The Title field is required.") }
                        }
                    }
                }
            };

            return Task.CompletedTask;
        });

        return options;
    }
}
