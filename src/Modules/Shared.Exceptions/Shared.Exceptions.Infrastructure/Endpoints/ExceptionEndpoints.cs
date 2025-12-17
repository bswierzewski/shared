using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Exceptions.Application.Commands.ThrowForbiddenException;
using Shared.Exceptions.Application.Commands.ThrowNotFoundException;
using Shared.Exceptions.Application.Commands.ThrowServerException;
using Shared.Exceptions.Application.Commands.ThrowUnauthorizedException;
using Shared.Exceptions.Application.Commands.ThrowValidationException;

namespace Shared.Exceptions.Infrastructure.Endpoints;

/// <summary>
/// Test endpoints for validating exception handling and ProblemDetails responses.
/// These endpoints execute commands that throw various exceptions to test the ApiExceptionHandler.
/// </summary>
public static class ExceptionEndpoints
{
    /// <summary>
    /// Maps test endpoints that throw different types of exceptions.
    /// Requires authorization and executes commands via MediatR.
    /// </summary>
    public static IEndpointRouteBuilder MapExceptionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/exceptions")
            .WithTags("Exceptions")
            .RequireAuthorization();

        group.MapGet("/validation", async (ISender sender) =>
            {
                // Pass invalid data to trigger validation errors via ValidationBehavior
                await sender.Send(new ThrowValidationExceptionCommand(
                    Title: null,      // Required field violation
                    Content: "",      // MinimumLength(10) violation
                    Tags: null        // NotEmpty violation
                ));
                return Results.Ok();
            })
            .WithName("ThrowValidationException")
            .Produces<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status400BadRequest)
            .WithSummary("Throws a ValidationException to test validation error responses");

        group.MapGet("/not-found", async (ISender sender) =>
            {
                await sender.Send(new ThrowNotFoundExceptionCommand());
                return Results.Ok();
            })
            .WithName("ThrowNotFoundException")
            .Produces<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status404NotFound)
            .WithSummary("Throws a NotFoundException to test 404 error responses");

        group.MapGet("/unauthorized", async (ISender sender) =>
            {
                await sender.Send(new ThrowUnauthorizedExceptionCommand());
                return Results.Ok();
            })
            .WithName("ThrowUnauthorizedException")
            .Produces<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status401Unauthorized)
            .WithSummary("Throws an UnauthorizedAccessException to test 401 error responses");

        group.MapGet("/forbidden", async (ISender sender) =>
            {
                await sender.Send(new ThrowForbiddenExceptionCommand());
                return Results.Ok();
            })
            .WithName("ThrowForbiddenException")
            .Produces<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status403Forbidden)
            .WithSummary("Throws a ForbiddenAccessException to test 403 error responses");

        group.MapGet("/server-error", async (ISender sender) =>
            {
                await sender.Send(new ThrowServerExceptionCommand());
                return Results.Ok();
            })
            .WithName("ThrowServerException")
            .Produces<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithSummary("Throws an unexpected exception to test 500 error responses");

        return endpoints;
    }
}
