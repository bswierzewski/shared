using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Users.Application.Options;
using Shared.Users.Infrastructure.Consts;

namespace Shared.Users.Infrastructure.Extensions.Supabase;

/// <summary>
/// Extension methods for setting up Supabase JWT authentication using ES256 signatures.
/// </summary>
public static class SupabaseJwtBearerExtensions
{
    /// <summary>
    /// Adds Supabase JWT Bearer authentication to the service collection.
    /// Configures the application to validate tokens signed with ES256 (ECDSA) keys fetched dynamically from Supabase.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="configureOptions">An action to configure the <see cref="JwtBearerOptions"/> further.</param>
    /// <returns>The modified <see cref="AuthenticationBuilder"/>.</returns>
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
                    if (principal?.Identity is ClaimsIdentity identity)
                    {
                        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
                        var email = principal.FindFirstValue(ClaimTypes.Email);

                        // Fallback logic for display name
                        var displayName = principal.FindFirstValue("user_metadata.name")
                            ?? principal.FindFirstValue("user_metadata.full_name")
                            ?? principal.FindFirstValue("name")
                            ?? email;

                        if (!string.IsNullOrEmpty(displayName) && principal.FindFirst(ClaimTypes.Name) == null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Name, displayName));
                        }

                        // Add specific provider claim
                        if (!string.IsNullOrEmpty(sub))
                        {
                            identity.AddClaim(new Claim(ClaimsConsts.Provider, "Supabase"));
                        }

                        // Map profile picture
                        var picture = principal.FindFirstValue("user_metadata.picture") ?? principal.FindFirstValue("picture");
                        if (!string.IsNullOrEmpty(picture))
                        {
                            identity.AddClaim(new Claim(ClaimsConsts.PictureUrl, picture));
                        }
                    }
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
