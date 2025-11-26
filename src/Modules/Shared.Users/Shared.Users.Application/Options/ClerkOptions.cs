using System.ComponentModel.DataAnnotations;
using Shared.Abstractions.Options;
using Shared.Users.Domain;

namespace Shared.Users.Application.Options;

/// <summary>
/// Configuration options for Clerk authentication provider.
/// </summary>
public class ClerkOptions : IOptions
{
    /// <summary>
    /// The configuration section name for Clerk options.
    /// </summary>
    public static string SectionName => $"Modules:{ModuleConstants.ModuleName}:Authentication:Clerk";

    /// <summary>
    /// The authority URL for Clerk (e.g., https://your-domain.clerk.accounts.dev).
    /// This is the issuer of the JWT tokens and the OpenID Connect discovery endpoint.
    /// </summary>
    [Required(ErrorMessage = "Authority is required")]
    [Url(ErrorMessage = "Authority must be a valid URL")]
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The audience for JWT token validation (optional).
    /// If not set, audience validation will be disabled.
    /// Corresponds to the 'aud' claim in the JWT token.
    /// </summary>
    public string? Audience { get; set; }
}
