using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(ProblemDetails))
                return Task.CompletedTask;

            schema.Description = "A machine-readable format for specifying errors in HTTP API responses based on RFC 7807.";
            schema.AdditionalPropertiesAllowed = true;

            // Enrich standard RFC 7807 properties
            SetProperty(schema, "type", "A URI reference that identifies the problem type.", "https://tools.ietf.org/html/rfc9110#section-15.5.1");
            SetProperty(schema, "title", "A short, human-readable summary of the problem type.", "Bad Request");
            SetProperty(schema, "status", "The HTTP status code.", 400);
            SetProperty(schema, "detail", "A human-readable explanation specific to this occurrence of the problem.", "The request contains invalid data.");
            SetProperty(schema, "instance", "A URI reference that identifies the specific occurrence of the problem.", "/api/snippets");

            // Add custom properties
            AddProperty(schema, "traceId", "string", "The trace identifier for correlating the error with logs.", "00-1234567890abcdef-1234567890abcdef-00");

            AddProperty(schema, "errors", "object", "Validation errors grouped by property name (only present for validation errors).",
                new OpenApiObject
                {
                    ["Title"] = new OpenApiArray { new OpenApiString("The Title field is required.") },
                    ["Content"] = new OpenApiArray { new OpenApiString("The Content field must be at least 10 characters long.") }
                },
                additionalProperties: new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } });

            return Task.CompletedTask;
        });

        return options;
    }

    private static void SetProperty(OpenApiSchema schema, string name, string description, object exampleValue)
    {
        if (!schema.Properties.TryGetValue(name, out var prop)) return;

        prop.Description = description;
        prop.Example = exampleValue switch
        {
            string s => new OpenApiString(s),
            int i => new OpenApiInteger(i),
            _ => null
        };
    }

    private static void AddProperty(OpenApiSchema schema, string name, string type, string description, object example, OpenApiSchema? additionalProperties = null)
    {
        if (schema.Properties.ContainsKey(name)) return;

        var property = new OpenApiSchema
        {
            Type = type,
            Nullable = true,
            Description = description,
            Example = example switch
            {
                string s => new OpenApiString(s),
                OpenApiObject obj => obj,
                _ => null
            }
        };

        if (additionalProperties != null)
            property.AdditionalProperties = additionalProperties;

        schema.Properties[name] = property;
    }
}
