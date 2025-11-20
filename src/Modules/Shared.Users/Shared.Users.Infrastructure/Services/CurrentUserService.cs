using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Abstractions.Authorization;
using Shared.Users.Infrastructure.Consts;

namespace Shared.Users.Infrastructure.Services;

/// <summary>
/// IUser implementation that provides access to the current authenticated user's claims.
/// Reads from the enriched ClaimsPrincipal populated by JitProvisioningMiddleware.
///
/// Flow:
/// 1. JWT Token arrives
/// 2. JitProvisioningMiddleware provisions user to DB
/// 3. Middleware adds roles/permissions from DB to ClaimsPrincipal
/// 4. This service reads from enriched ClaimsPrincipal
/// </summary>
internal class CurrentUserService : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal ClaimsPrincipal
        => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    /// <summary>
    /// Internal user ID (internal GUID set by JitProvisioningMiddleware)
    /// </summary>
    public Guid? Id
    {
        get
        {
            var userIdClaim = ClaimsPrincipal.FindFirst(ClaimsConsts.UserId)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : null;
        }
    }

    /// <summary>
    /// Email from JWT or database
    /// </summary>
    public string? Email => ClaimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Display name from JWT or database
    /// </summary>
    public string? FullName => ClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Picture URL from JWT or database
    /// </summary>
    public string? PictureUrl => ClaimsPrincipal.FindFirst(ClaimsConsts.PictureUrl)?.Value;

    /// <summary>
    /// Is user authenticated (has valid JWT token)
    /// </summary>
    public bool IsAuthenticated => ClaimsPrincipal.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// All claims from enriched ClaimsPrincipal
    /// </summary>
    public IEnumerable<string> Claims
        => ClaimsPrincipal.Claims.Select(c => $"{c.Type}:{c.Value}") ?? Enumerable.Empty<string>();

    /// <summary>
    /// Roles from ClaimTypes.Role claims (enriched by middleware from database)
    /// </summary>
    public IEnumerable<string> Roles
        => ClaimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    /// <summary>
    /// Permissions from permission claims (enriched by middleware from database).
    /// Includes both direct permissions and those assigned via roles.
    /// </summary>
    public IEnumerable<string> Permissions
        => ClaimsPrincipal.FindAll(ClaimsConsts.Permission).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsInRole(string role)
        => Roles.Contains(role);

    public bool HasClaim(string claimType, string? claimValue = null)
    {
        var claim = ClaimsPrincipal.FindFirst(claimType);
        return claimValue == null ? claim != null : claim?.Value == claimValue;
    }

    public bool HasPermission(string permission)
        => Permissions.Contains(permission);
}
