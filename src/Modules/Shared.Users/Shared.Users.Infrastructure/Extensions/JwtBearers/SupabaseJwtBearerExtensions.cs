using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Users.Application.Options;
using Shared.Users.Infrastructure.Consts;

namespace Shared.Users.Infrastructure.Extensions.JwtBearers;

/// <summary>
/// Extension methods for adding Supabase JWT Bearer authentication.
///
/// Supabase uses HS256 symmetric key signing with a JWT secret.
/// Token validation is performed by JwtBearer middleware.
/// JIT provisioning and claims enrichment are handled by JitProvisioningMiddleware.
/// </summary>
public static class SupabaseJwtBearerExtensions
{
    /// <summary>
    /// Adds Supabase JWT Bearer authentication.
    /// Configures JWT validation using HS256 symmetric key and handles
    /// Supabase-specific claim mappings (user_metadata.name fallback, etc).
    ///
    /// JIT provisioning and claims enrichment are handled by JitProvisioningMiddleware,
    /// which runs after authentication and database lookup of roles/permissions.
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="configureOptions">Optional additional JWT Bearer configuration</param>
    /// <returns>The authentication builder for chaining</returns>
    public static AuthenticationBuilder AddSupabaseJwtBearer(
        this AuthenticationBuilder builder,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        // Add JWT Bearer authentication scheme
        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = true;

            // Configure token validation parameters for HS256
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = false, // May be disabled if not configured
                ClockSkew = TimeSpan.FromMinutes(5) // Allow some clock skew for edge cases
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

                    // Extract standard claims
                    var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
                    var email = principal.FindFirstValue(ClaimTypes.Email);

                    // Extract display name with fallback logic for Supabase-specific claims
                    // Supabase stores user metadata in 'user_metadata' claim which may contain 'name' or 'full_name'
                    var displayName = principal.FindFirstValue("user_metadata.name")
                        ?? principal.FindFirstValue(ClaimTypes.Name)
                        ?? principal.FindFirstValue("name")
                        ?? principal.FindFirstValue("user_metadata.full_name")
                        ?? email;

                    // Map displayName claim if we found a value
                    if (!string.IsNullOrEmpty(displayName) && principal.FindFirst(ClaimTypes.Name) == null)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, displayName));
                    }

                    // Add provider identifier claim for JitProvisioningMiddleware
                    if (!string.IsNullOrEmpty(sub))
                    {
                        identity.AddClaim(new Claim(ClaimsConsts.Provider, "Supabase"));
                    }

                    // Supabase may provide user metadata with additional information
                    // Extract picture URL if available in user_metadata
                    var picture = principal.FindFirstValue("user_metadata.picture")
                        ?? principal.FindFirstValue("picture");
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

        // Configure HS256 signing key, issuer, and audience from SupabaseOptions
        builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<SupabaseOptions>>((jwtOptions, supabaseOptions) =>
            {
                // Create symmetric security key from JWT secret
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseOptions.Value.JwtSecret));
                jwtOptions.TokenValidationParameters.IssuerSigningKey = key;

                // Set issuer validation
                // Supabase issuer is typically {authority}/auth/v1
                jwtOptions.TokenValidationParameters.ValidIssuer = $"{supabaseOptions.Value.Authority}/auth/v1";

                // Configure audience validation if provided
                if (!string.IsNullOrWhiteSpace(supabaseOptions.Value.Audience))
                {
                    jwtOptions.TokenValidationParameters.ValidAudience = supabaseOptions.Value.Audience;
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
