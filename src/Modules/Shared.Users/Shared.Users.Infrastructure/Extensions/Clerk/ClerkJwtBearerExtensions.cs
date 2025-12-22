using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Users.Application.Options;
using Shared.Users.Infrastructure.Consts;
using System.Security.Claims;

namespace Shared.Users.Infrastructure.Extensions.Clerk;

public static class ClerkJwtBearerExtensions
{
    /// <summary>
    /// Adds Clerk JWT Bearer authentication.
    /// Configures JWT validation using OpenID Connect discovery.
    /// </summary>
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
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Configure events to handle provider-specific claims mapping
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var principal = context.Principal;
                    if (principal?.Identity is not ClaimsIdentity identity)
                    {
                        context.Fail("Identity is missing.");
                        return Task.CompletedTask;
                    }

                    // 1. Walidacja SUB (ExternalId)
                    var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
                    if (string.IsNullOrEmpty(sub))
                    {
                        context.Fail("Claim 'sub' is required.");
                        return Task.CompletedTask;
                    }

                    // 2. Walidacja EMAIL
                    var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
                    if (string.IsNullOrEmpty(email))
                    {
                        context.Fail("Claim 'email' is required.");
                        return Task.CompletedTask;
                    }

                    identity.AddClaim(new Claim(ClaimsConsts.ExternalId, sub));
                    identity.AddClaim(new Claim(ClaimsConsts.Email, email));
                    identity.AddClaim(new Claim(ClaimsConsts.Provider, "Clerk"));

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
