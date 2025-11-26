using System.ComponentModel.DataAnnotations;
using Shared.Abstractions.Options;
using Shared.Users.Domain;

namespace Shared.Users.Application.Options;

/// <summary>
/// Configuration options for Supabase authentication provider.
/// </summary>
public class SupabaseOptions : IOptions
{
    /// <summary>
    /// The configuration section name for Supabase options.
    /// </summary>
    public static string SectionName => $"Modules:{ModuleConstants.ModuleName}:Authentication:Supabase";

    /// <summary>
    /// The Supabase project URL (e.g., https://your-project.supabase.co).
    /// This is the issuer of the JWT tokens and base URL for auth endpoints.
    /// </summary>
    [Required(ErrorMessage = "Authority is required")]
    [Url(ErrorMessage = "Authority must be a valid URL")]
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The audience for JWT token validation (optional).
    /// If not set, audience validation will be disabled.
    /// Supabase typically uses 'authenticated' as the audience.
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// The JWT secret key for HS256 signature validation.
    /// This is the secret key from your Supabase project settings.
    /// Must be kept secret and stored securely (e.g., in Azure Key Vault).
    /// </summary>
    [Required(ErrorMessage = "JwtSecret is required")]
    public string JwtSecret { get; set; } = null!;
}
