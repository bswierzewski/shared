using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Exceptions.Application.Commands;
using Shared.Infrastructure.Extensions;

namespace Shared.Exceptions.Infrastructure.Endpoints;

/// <summary>
/// Extension methods for mapping exception testing endpoints.
/// Provides HTTP endpoints for testing different error handling scenarios.
/// </summary>
public static class ExceptionEndpoints
{
    /// <summary>
    /// Maps all HTTP endpoints for exception testing operations.
    /// Establishes the routing structure for the Exceptions API under the "/api/exceptions" base path.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder used to configure API routes</param>
    public static void MapExceptionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/exceptions")
            .WithTags("Exceptions")
            .RequireAuthorization();

        // POST /api/exceptions/unhandled-error - Tests unhandled error handling
        group.MapPost("/unhandled-error", UnhandledError)
            .WithName("UnhandledError")
            .WithDescription("Tests unhandled error handling")
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // POST /api/exceptions/success - Tests successful response
        group.MapPost("/success", SuccessResponse)
            .WithName("SuccessResponse")
            .WithDescription("Tests successful response handling")
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // POST /api/exceptions/error - Tests error response
        group.MapPost("/error", ErrorResponse)
            .WithName("ErrorResponse")
            .WithDescription("Tests error response handling")
            .Produces<string>(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // POST /api/exceptions/role-protected - Tests role-based access control
        // Authorization is enforced via [Authorize] attribute on RoleProtectedCommand
        group.MapPost("/role-protected", RoleProtected)
            .WithName("RoleProtected")
            .WithDescription("Tests role-based access control - requires admin role")
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> UnhandledError(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnhandledErrorCommand(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> SuccessResponse(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SuccessResponseCommand(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ErrorResponse(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ErrorResponseCommand(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RoleProtected(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RoleProtectedCommand(), cancellationToken);
        return result.ToHttpResult();
    }
}
