using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Users.Infrastructure.Consts;

namespace Shared.Users.Tests.Authentication;

/// <summary>
/// Extension methods for adding test JWT Bearer authentication.
///
/// Replicates production JWT Bearer setup from SupabaseJwtBearerExtensions and ClerkJwtBearerExtensions,
/// but with token validation disabled for testing purposes.
///
/// Disables validation of:
/// - Token lifetime (expiry)
/// - Issuer signing key (signature)
/// - Issuer
/// - Audience
///
/// But preserves real JWT parsing and claims mapping for authentic test scenarios.
/// </summary>
public static class TestJwtBearerExtensions
{
    /// <summary>
    /// Adds test JWT Bearer authentication with validation disabled.
    /// Preserves real JWT parsing and claims mapping logic from production.
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <returns>The authentication builder for chaining</returns>
    public static AuthenticationBuilder AddTestJwtBearer(this AuthenticationBuilder builder)
    {
        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = false; // Allow HTTP for tests

            // Disable all validation - we're testing business logic, not JWT validation
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = false,                  // Allow expired tokens
                ValidateIssuerSigningKey = false,          // Don't validate signature
                ValidateIssuer = false,                    // Don't validate issuer
                ValidateAudience = false,                  // Don't validate audience
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("test-secret-test-secret-test-secret-test-secret"))
            };

            // Preserve claims mapping logic from production (OnTokenValidated event)
            // This ensures claims are mapped identically to production
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var principal = context.Principal;
                    if (principal?.Identity is not ClaimsIdentity identity)
                    {
                        return Task.CompletedTask;
                    }

                    // Extract standard claims - match production logic
                    var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? principal.FindFirstValue("sub");

                    var email = principal.FindFirstValue(ClaimTypes.Email)
                        ?? principal.FindFirstValue("email");

                    // Extract display name with fallback - match Supabase logic
                    var displayName = principal.FindFirstValue("user_metadata.name")
                        ?? principal.FindFirstValue(ClaimTypes.Name)
                        ?? principal.FindFirstValue("name")
                        ?? principal.FindFirstValue("user_metadata.full_name")
                        ?? email;

                    // Map displayName claim if found
                    if (!string.IsNullOrEmpty(displayName) && principal.FindFirst(ClaimTypes.Name) == null)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, displayName));
                    }

                    // Add provider identifier claim for JitProvisioningMiddleware
                    if (!string.IsNullOrEmpty(sub))
                    {
                        identity.AddClaim(new Claim(ClaimsConsts.Provider, "Test"));
                    }

                    // Extract picture URL if available
                    var picture = principal.FindFirstValue("user_metadata.picture")
                        ?? principal.FindFirstValue("picture");
                    if (!string.IsNullOrEmpty(picture))
                    {
                        identity.AddClaim(new Claim(ClaimsConsts.PictureUrl, picture));
                    }

                    return Task.CompletedTask;
                }
            };
        });

        return builder;
    }
}
