using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Users.Application.Commands;
using Shared.Users.Application.DTOs;
using Shared.Users.Application.Queries;

namespace Shared.Users.Infrastructure.Endpoints;

/// <summary>
/// Extension methods for mapping user management endpoints.
/// Provides HTTP endpoints for user management operations including retrieval, role management, and permission grants.
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Maps all HTTP endpoints for user management operations.
    /// Establishes the routing structure for the Users API under the "/api/users" base path.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder used to configure API routes</param>
    /// <remarks>
    /// All endpoints are configured with OpenAPI support and require authorization.
    /// The "Users" tag groups these endpoints in the Swagger UI for better organization.
    /// </remarks>
    public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Create a route group for all user endpoints with consistent base path and tagging
        // This approach ensures API consistency and simplifies routing management
        var group = endpoints.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(); // All user endpoints require authentication

        // GET /api/users/{userId} - Query to get a user by ID
        // Returns user profile including roles and external provider mappings
        group.MapGet("/{userId}", GetUserById)
            .WithName("GetUserById")
            .WithDescription("Get a user by ID with their profile and external providers")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // POST /api/users/{userId}/roles/{roleName} - Command to assign a role to a user
        // Requires appropriate permissions for user management (users.assign_roles)
        group.MapPost("/{userId}/roles/{roleName}", AssignRoleToUser)
            .WithName("AssignRoleToUser")
            .WithDescription("Assign a role to a user (requires users.assign_roles permission)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/users/{userId}/roles/{roleName} - Command to remove a role from a user
        // Requires appropriate permissions for user management (users.assign_roles)
        group.MapDelete("/{userId}/roles/{roleName}", RemoveRoleFromUser)
            .WithName("RemoveRoleFromUser")
            .WithDescription("Remove a role from a user (requires users.assign_roles permission)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/users/{userId}/permissions/{permissionName} - Command to grant a direct permission
        // Separate from role-based permissions, allows granting specific permissions directly to a user
        // Optional query parameter: reason (reason for granting the permission)
        // Requires appropriate permissions for permission management (users.manage_permissions)
        group.MapPost("/{userId}/permissions/{permissionName}", GrantPermissionToUser)
            .WithName("GrantPermissionToUser")
            .WithDescription("Grant a direct permission to a user (requires users.manage_permissions permission)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/users/{userId}/permissions/{permissionName} - Command to revoke a direct permission
        // Revokes a directly granted permission from a user (separate from role-based revocation)
        // Requires appropriate permissions for permission management (users.manage_permissions)
        group.MapDelete("/{userId}/permissions/{permissionName}", RevokePermissionFromUser)
            .WithName("RevokePermissionFromUser")
            .WithDescription("Revoke a direct permission from a user (requires users.manage_permissions permission)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// Returns complete user profile including roles and external provider mappings.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve</param>
    /// <param name="mediator">MediatR instance for executing the query through the application layer</param>
    /// <returns>HTTP 200 OK with user details or 404 if user not found</returns>
    /// <remarks>
    /// This endpoint requires users.view permission to execute.
    /// The returned UserDto includes all profile information and external provider linkages.
    /// </remarks>
    private static async Task<IResult> GetUserById(
        Guid userId,
        IMediator mediator)
    {
        // Execute the query through the application layer
        var query = new GetUserByIdQuery(userId);
        var result = await mediator.Send(query);

        // Return 200 OK with user data or 404 if not found
        if (result == null)
            return Results.NotFound();

        return Results.Ok(result);
    }

    /// <summary>
    /// Assigns a specific role to a user, granting them all permissions associated with that role.
    /// This endpoint requires appropriate user management permissions to execute.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to whom the role will be assigned</param>
    /// <param name="roleName">The name of the role to assign (e.g., "admin", "editor", "viewer")</param>
    /// <param name="mediator">MediatR instance for executing the command through the application layer</param>
    /// <returns>HTTP 200 OK when role is successfully assigned</returns>
    /// <remarks>
    /// Role assignment is idempotent - assigning an already assigned role will not cause errors.
    /// Changes take effect after the database is updated.
    /// This operation requires users.assign_roles permission.
    /// </remarks>
    private static async Task<IResult> AssignRoleToUser(
        Guid userId,
        string roleName,
        IMediator mediator)
    {
        // Execute the assign role command through the application layer
        // Authorization checks are performed by AuthorizationBehavior
        var command = new AssignRoleToUserCommand(userId, roleName);
        await mediator.Send(command);

        // Return 200 OK on success
        return Results.Ok();
    }

    /// <summary>
    /// Removes a specific role from a user, revoking all permissions associated with that role.
    /// This endpoint requires appropriate user management permissions to execute.
    /// </summary>
    /// <param name="userId">The unique identifier of the user from whom the role will be removed</param>
    /// <param name="roleName">The name of the role to remove (e.g., "admin", "editor", "viewer")</param>
    /// <param name="mediator">MediatR instance for executing the command through the application layer</param>
    /// <returns>HTTP 200 OK when role is successfully removed</returns>
    /// <remarks>
    /// Role removal is idempotent - removing an already removed role will not cause errors.
    /// Changes take effect after the database is updated.
    /// This operation requires users.assign_roles permission.
    /// </remarks>
    private static async Task<IResult> RemoveRoleFromUser(
        Guid userId,
        string roleName,
        IMediator mediator)
    {
        // Execute the remove role command through the application layer
        // Authorization checks are performed by AuthorizationBehavior
        var command = new RemoveRoleFromUserCommand(userId, roleName);
        await mediator.Send(command);

        // Return 200 OK on success
        return Results.Ok();
    }

    /// <summary>
    /// Grants a direct permission to a user, separate from role-based permissions.
    /// Useful for fine-grained access control beyond role assignments.
    /// This endpoint requires appropriate permission management permissions to execute.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to whom the permission will be granted</param>
    /// <param name="permissionName">The name of the permission to grant (e.g., "users.view", "users.edit")</param>
    /// <param name="reason">Optional reason for granting this permission</param>
    /// <param name="mediator">MediatR instance for executing the command through the application layer</param>
    /// <returns>HTTP 200 OK when permission is successfully granted</returns>
    /// <remarks>
    /// Permission grant is idempotent - granting an already granted permission will not cause errors.
    /// Direct permissions are separate from role-based permissions and provide granular control.
    /// Changes take effect after the database is updated.
    /// This operation requires users.manage_permissions permission.
    /// The reason parameter is optional and is recorded for audit purposes.
    /// </remarks>
    private static async Task<IResult> GrantPermissionToUser(
        Guid userId,
        string permissionName,
        [FromQuery] string? reason,
        IMediator mediator)
    {
        // Execute the grant permission command through the application layer
        // Authorization checks are performed by AuthorizationBehavior
        var command = new GrantPermissionToUserCommand(userId, permissionName, reason);
        await mediator.Send(command);

        // Return 200 OK on success
        return Results.Ok();
    }

    /// <summary>
    /// Revokes a direct permission from a user.
    /// Removes a previously granted direct permission, separate from role-based revocation.
    /// This endpoint requires appropriate permission management permissions to execute.
    /// </summary>
    /// <param name="userId">The unique identifier of the user from whom the permission will be revoked</param>
    /// <param name="permissionName">The name of the permission to revoke (e.g., "users.view", "users.edit")</param>
    /// <param name="mediator">MediatR instance for executing the command through the application layer</param>
    /// <returns>HTTP 200 OK when permission is successfully revoked</returns>
    /// <remarks>
    /// Permission revocation is idempotent - revoking an already revoked permission will not cause errors.
    /// Only affects directly granted permissions, not role-based permissions.
    /// Changes take effect after the database is updated.
    /// This operation requires users.manage_permissions permission.
    /// </remarks>
    private static async Task<IResult> RevokePermissionFromUser(
        Guid userId,
        string permissionName,
        IMediator mediator)
    {
        // Execute the revoke permission command through the application layer
        // Authorization checks are performed by AuthorizationBehavior
        var command = new RevokePermissionFromUserCommand(userId, permissionName);
        await mediator.Send(command);

        // Return 200 OK on success
        return Results.Ok();
    }
}
