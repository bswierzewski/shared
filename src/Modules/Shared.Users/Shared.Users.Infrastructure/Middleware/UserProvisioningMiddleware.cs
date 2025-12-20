using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain.Enums;
using Shared.Users.Infrastructure.Consts;
using System.Security.Claims;

namespace Shared.Users.Infrastructure.Middleware;

/// <summary>
/// User Provisioning Middleware - orchestrates user provisioning and claims enrichment.
///
/// Flow:
/// 1. Extract JWT claims (email, sub, displayName, provider)
///    Note: Picture URL comes from the JWT token and is NOT stored in the domain
/// 2. Call IUserProvisioningService.UpsertUserAsync() for JIT user provisioning
/// 3. Replace NameIdentifier claim with internal user ID (GUID)
/// 4. Load user's assigned roles and permissions from database
/// 5. Add role and permission claims to ClaimsPrincipal for authorization
/// </summary>
public class UserProvisioningMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserProvisioningMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the UserProvisioningMiddleware
    /// </summary>
    public UserProvisioningMiddleware(
        RequestDelegate next,
        ILogger<UserProvisioningMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes the request and provisions the user if authenticated via JWT
    /// </summary>
    public async Task InvokeAsync(
        HttpContext context,
        IUserProvisioningService provisioningService,
        IUsersDbContext dbContext)
    {
        var claimsPrincipal = context.User;

        if (!claimsPrincipal.Identity?.IsAuthenticated ?? false)
        {
            await _next(context);
            return;
        }

        try
        {
            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                ?? claimsPrincipal.FindFirst("email")?.Value;

            var sub = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            var displayName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value
                ?? claimsPrincipal.FindFirst("name")?.Value;

            var providerStr = claimsPrincipal.FindFirst(ClaimsConsts.Provider)?.Value;

            if (string.IsNullOrEmpty(sub))
            {
                _logger.LogWarning("JWT token missing NameIdentifier claim");
                await _next(context);
                return;
            }

            // Provider must be set by the authentication extension (Clerk, Supabase, etc.)
            if (string.IsNullOrEmpty(providerStr))
            {
                _logger.LogWarning("JWT token missing provider claim - ensure authentication extension is properly configured");
                await _next(context);
                return;
            }

            if (!Enum.TryParse<IdentityProvider>(providerStr, ignoreCase: true, out var provider))
            {
                _logger.LogWarning("Unknown identity provider: {Provider}", providerStr);
                provider = IdentityProvider.Other;
            }

            var user = await provisioningService.UpsertUserAsync(
                provider, sub, email, displayName);

            var identity = claimsPrincipal.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var nameIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (nameIdClaim != null)
                {
                    identity.RemoveClaim(nameIdClaim);
                }

                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                identity.AddClaim(new Claim(ClaimsConsts.UserId, user.Id.ToString()));
            }

            var userFull = await dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == user.Id)
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .Include(u => u.Permissions)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (identity != null && userFull != null)
            {
                // Extract assigned role names (from Role entities)
                var roleNames = userFull.Roles
                    .Where(r => r.IsActive) // Only active roles
                    .Select(r => r.Name)
                    .Distinct()
                    .ToList();

                foreach (var roleName in roleNames)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }

                // Extract all permission names (direct grants + from roles)
                var directPermissions = userFull.Permissions
                    .Where(p => p.IsActive) // Only active permissions
                    .Select(p => p.Name)
                    .ToHashSet();

                // Add permissions from assigned roles
                var rolePermissions = userFull.Roles
                    .Where(r => r.IsActive)
                    .SelectMany(r => r.Permissions)
                    .Where(p => p.IsActive)
                    .Select(p => p.Name)
                    .ToHashSet();

                // Combine both sets
                var allPermissions = directPermissions.Union(rolePermissions).ToList();

                foreach (var permission in allPermissions)
                {
                    identity.AddClaim(new Claim(ClaimsConsts.Permission, permission));
                }

                _logger.LogInformation("JIT provisioned user {UserId} ({Email}) with {RoleCount} roles and {PermissionCount} permissions",
                    user.Id, email, roleNames.Count, allPermissions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in user provisioning middleware");
            // Fail-open: continue request without provisioning
        }

        await _next(context);
    }
}
