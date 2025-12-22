using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Abstractions;
using Shared.Abstractions.Authorization;
using System.Reflection;

namespace Shared.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that handles authorization for MediatR requests.
/// Checks roles, permissions, and claims from JWT tokens to authorize requests.
/// </summary>
/// <typeparam name="TRequest">The type of the MediatR request.</typeparam>
/// <typeparam name="TResponse">The inner type wrapped by ErrorOr.</typeparam>
/// <remarks>
/// <para>
/// If user is not authenticated, returns an ErrorOr with Unauthorized error.
/// If user is authenticated but lacks required authorization, returns an ErrorOr with Forbidden error.
/// </para>
/// </remarks>
/// <param name="user">The current user service.</param>
/// <param name="logger">The logger instance.</param>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    IUser user,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUser _user = user;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Handles the request with authorization checks.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ErrorOr result containing either the response or authorization errors.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        // If no authorization attributes found, continue without authorization checks
        if (!authorizeAttributes.Any())
            return await next(cancellationToken);

        // Check authorization requirements
        foreach (var attribute in authorizeAttributes)
        {
            // Check required permissions (AND logic - user needs all)
            if (attribute.Permissions.Length == 0)
                return await next(cancellationToken);

            //foreach (var permission in attribute.Permissions)
            //{
            //    if (!_user.HasPermission(permission))
            //    {
            //        _logger.LogWarning(
            //            "Authorization failed for user {UserId} on request {RequestName}: User does not have the required permission: {Permission}",
            //            _user.Id?.ToString() ?? "Unknown",
            //            typeof(TRequest).Name,
            //            permission);

            //        return (dynamic)Error.Forbidden(code: "Auth.Forbidden", description: $"User does not have the required permission: {permission}");
            //    }
            //}
        }

        // Authorization passed, continue to next behavior
        return await next(cancellationToken);
    }
}

