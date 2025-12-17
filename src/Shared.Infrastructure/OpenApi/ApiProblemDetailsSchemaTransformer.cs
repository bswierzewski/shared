using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace Shared.Infrastructure.OpenApi;

/// <summary>
/// Transforms the ProblemDetails schema to include traceId and errors fields.
/// </summary>
public sealed class ApiProblemDetailsSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <summary>
    /// Transforms the schema to add custom properties for API problem details.
    /// </summary>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        // Check if this is a ProblemDetails or derived type
        if (context.JsonTypeInfo.Type.IsAssignableTo(typeof(ProblemDetails)))
        {
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();

            // Add traceId property (nullable string) if not already present
            if (!schema.Properties.ContainsKey("traceId"))
            {
                schema.Properties["traceId"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String | JsonSchemaType.Null,
                    Description = "The trace identifier for request tracking and debugging."
                };
            }

            // Add errors property for validation errors (nullable object) if not already present
            if (!schema.Properties.ContainsKey("errors"))
            {
                schema.Properties["errors"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object | JsonSchemaType.Null,
                    Description = "Validation errors grouped by field name. Null if no validation errors.",
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String
                        }
                    }
                };
            }
        }

        return Task.CompletedTask;
    }
}
