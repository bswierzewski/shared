using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Users.Application.Options;
using Shared.Users.Infrastructure.Consts;

namespace Shared.Users.Infrastructure.Extensions.JwtBearers;

/// <summary>
/// Extension methods for adding Clerk JWT Bearer authentication.
///
/// Clerk uses OpenID Connect with JWKS discovery.
/// Token validation is performed by JwtBearer middleware.
/// JIT provisioning and claims enrichment are handled by JitProvisioningMiddleware.
/// </summary>
public static class ClerkJwtBearerExtensions
{
    /// <summary>
    /// Adds Clerk JWT Bearer authentication.
    /// Configures JWT validation using OpenID Connect discovery.
    ///
    /// JIT provisioning and claims enrichment are handled by JitProvisioningMiddleware,
    /// which runs after authentication and database lookup of roles/permissions.
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="configureOptions">Optional additional JWT Bearer configuration</param>
    /// <returns>The authentication builder for chaining</returns>
    public static AuthenticationBuilder AddClerkJwtBearer(
        this AuthenticationBuilder builder,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        // Add JWT Bearer authentication scheme
        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            // Configure token validation parameters
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = false, // May be disabled if not configured
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            // Configure events to handle provider-specific claims mapping
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // Extract claims from JWT token
                    var principal = context.Principal;
                    if (principal?.Identity is not ClaimsIdentity identity)
                    {
                        return Task.CompletedTask;
                    }

                    // Extract standard claims - Clerk provides these consistently
                    var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
                    var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                    var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.FindFirst("name")?.Value;

                    // Add provider identifier claim for JitProvisioningMiddleware
                    if (!string.IsNullOrEmpty(sub))
                    {
                        identity.AddClaim(new Claim(ClaimsConsts.Provider, "Clerk"));
                    }

                    // Clerk provides picture URL in 'picture' claim
                    var picture = principal.FindFirst("picture")?.Value;
                    if (!string.IsNullOrEmpty(picture))
                    {
                        identity.AddClaim(new Claim(ClaimsConsts.PictureUrl, picture));
                    }

                    return Task.CompletedTask;
                }
            };

            // Allow caller to provide additional configuration
            configureOptions?.Invoke(options);
        });

        // Configure authority and audience from ClerkOptions
        builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<ClerkOptions>>((jwtOptions, clerkOptions) =>
            {
                // Clerk uses OpenID Connect with JWKS discovery
                // Authority is used for discovering the JWKS endpoint
                jwtOptions.Authority = clerkOptions.Value.Authority;

                // Configure audience validation if provided
                if (!string.IsNullOrEmpty(clerkOptions.Value.Audience))
                {
                    jwtOptions.TokenValidationParameters.ValidAudience = clerkOptions.Value.Audience;
                    jwtOptions.TokenValidationParameters.ValidateAudience = true;
                }
                else
                {
                    jwtOptions.TokenValidationParameters.ValidateAudience = false;
                }
            });

        return builder;
    }
}
