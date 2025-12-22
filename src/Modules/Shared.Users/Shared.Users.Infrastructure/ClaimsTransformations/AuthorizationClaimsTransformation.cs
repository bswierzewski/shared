using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Enums;
using Shared.Users.Infrastructure.Consts;
using System.Security.Claims;

namespace Shared.Users.Infrastructure.ClaimsTransformations;

public class AuthorizationClaimsTransformation(
    IServiceScopeFactory scopeFactory,
    IMemoryCache cache,
    ILogger<AuthorizationClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not { IsAuthenticated: true } || principal.HasClaim(c => c.Type == ClaimsConsts.UserId))
            return principal;

        var externalId = principal.FindFirstValue(ClaimsConsts.ExternalId);
        var email = principal.FindFirstValue(ClaimsConsts.Email);
        var provider = principal.FindFirstValue(ClaimsConsts.Provider);

        if (string.IsNullOrEmpty(externalId) || string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(email))
        {
            logger.LogWarning("Missing essential claims (sub/provider/email) for user transformation.");
            return principal;
        }

        string cacheKey = $"user_claims_{externalId}_{provider}";

        if (!cache.TryGetValue(cacheKey, out List<Claim>? enrichedClaims))
        {
            using var scope = scopeFactory.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            if (!Enum.TryParse<IdentityProvider>(provider, true, out var identityProvider))
                identityProvider = IdentityProvider.Other;

            var user = await userService.GetByExternalIdAsync(identityProvider, externalId);

            if (user == null)
            {
                user = await userService.CreateAsync(identityProvider, externalId, email);

                logger.LogInformation("JIT user provisioned: {InternalId}, {Email} and {Provider}:{ExternalId}", user.Id, user.Email, identityProvider, externalId);
            }
            else            
                logger.LogInformation("JIT user enriched: {InternalId}, {Email} and {Provider}:{ExternalId}", user.Id, user.Email, identityProvider, externalId);            

            enrichedClaims = BuildClaims(user);

            cache.Set(cacheKey, enrichedClaims, TimeSpan.FromMinutes(10));
        }

        var appIdentity = new ClaimsIdentity();
        appIdentity.AddClaims(enrichedClaims!);
        principal.AddIdentity(appIdentity);

        return principal;
    }

    private static List<Claim> BuildClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimsConsts.UserId, user.Id.ToString()),
            new(ClaimsConsts.Email, user.Email)
        };

        var activeRoles = user.Roles.Where(r => r.IsActive).ToList();
        foreach (var role in activeRoles)
        {
            claims.Add(new Claim(ClaimsConsts.Role, role.Name));

            var activePermissions = role.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.Name)
                .Distinct();

            foreach (var permission in activePermissions)
                claims.Add(new Claim(ClaimsConsts.Permission, permission));
        }

        return claims;
    }
}