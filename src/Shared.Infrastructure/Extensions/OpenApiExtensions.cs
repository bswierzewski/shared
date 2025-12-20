using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring OpenAPI with enhanced ProblemDetails schemas.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds schema transformers to include traceId and errors fields in ProblemDetails schemas.
    /// This ensures that Orval and other OpenAPI consumers are aware of these fields.
    /// </summary>
    public static OpenApiOptions AddProblemDetailsSchemas(this OpenApiOptions options)
    {
        options.AddSchemaTransformer(async (schema, context, cancellationToken) =>
        {
            if (!context.JsonTypeInfo.Type.IsAssignableTo(typeof(ProblemDetails)) || schema.Properties is null)
                return;

            if (!schema.Properties.ContainsKey("traceId"))
            {
                var traceIdSchema = await context.GetOrCreateSchemaAsync(
                    type: typeof(string),
                    parameterDescription: null,
                    cancellationToken: cancellationToken);

                traceIdSchema.Description = "The trace identifier for request tracking and debugging.";
                traceIdSchema.ReadOnly = true;

                schema.Properties["traceId"] = traceIdSchema;
            }

            if (!schema.Properties.ContainsKey("errors"))
            {
                var errorsSchema = await context.GetOrCreateSchemaAsync(
                    type: typeof(Dictionary<string, string[]>),
                    parameterDescription: null,
                    cancellationToken: cancellationToken);

                errorsSchema.Description = "Validation errors grouped by field name.";

                schema.Properties["errors"] = errorsSchema;
            }
        });

        return options;
    }
}
