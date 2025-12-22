using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Users.Application.Options;
using Shared.Users.Infrastructure.Consts;
using System.Security.Claims;

namespace Shared.Users.Infrastructure.Extensions.Supabase;

public static class SupabaseJwtBearerExtensions
{
    /// <summary>
    /// Adds Supabase JWT Bearer authentication to the service collection.
    /// Configures the application to validate tokens signed with ES256 (ECDSA) keys fetched dynamically from Supabase.
    /// </summary>
    public static AuthenticationBuilder AddSupabaseJwtBearer(
        this AuthenticationBuilder builder,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        // Register the KeyManager as a Singleton to maintain the lifecycle of the ConfigurationManager cache
        builder.Services.TryAddSingleton<SupabaseKeyManager>();

        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateAudience = false, // Supabase set always [authenticated]
                ValidateIssuerSigningKey = true, // Essential for triggering the IssuerSigningKeyResolver
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Configure events for claims enrichment and mapping
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

                    var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
                    if (string.IsNullOrEmpty(sub))
                    {
                        context.Fail("Claim 'sub' is required.");
                        return Task.CompletedTask;
                    }

                    var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
                    if (string.IsNullOrEmpty(email))
                    {
                        context.Fail("Claim 'email' is required.");
                        return Task.CompletedTask;
                    }

                    identity.AddClaim(new Claim(ClaimsConsts.ExternalId, sub));
                    identity.AddClaim(new Claim(ClaimsConsts.Email, email));
                    identity.AddClaim(new Claim(ClaimsConsts.Provider, "Supabase"));

                    return Task.CompletedTask;
                }
            };

            configureOptions?.Invoke(options);
        });

        // Use the Options Pattern to inject SupabaseOptions and SupabaseKeyManager into JwtBearerOptions
        builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<SupabaseOptions>, SupabaseKeyManager>((jwtOptions, supabaseOptions, keyManager) =>
            {
                var authority = supabaseOptions.Value.Authority;

                // Set the valid issuer (typically: https://<project>.supabase.co/auth/v1)
                jwtOptions.TokenValidationParameters.ValidIssuer = $"{authority}/auth/v1";

                // Assign the custom resolver that uses the KeyManager
                jwtOptions.TokenValidationParameters.IssuerSigningKeyResolver =
                    (token, securityToken, kid, validationParameters) =>
                    {
                        // Delegate key retrieval to the manager, passing the Key ID (kid) for rotation checks
                        return keyManager.GetSigningKeys(kid);
                    };
            });

        return builder;
    }
}
