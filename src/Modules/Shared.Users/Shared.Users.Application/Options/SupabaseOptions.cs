using System.ComponentModel.DataAnnotations;
using Shared.Abstractions.Abstractions;
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
    public static string SectionName => $"Modules:{Module.Name}:Supabase";

    /// <summary>
    /// The Supabase project URL (e.g., https://your-project.supabase.co).
    /// This is the issuer of the JWT tokens and base URL for auth endpoints.
    /// </summary>
    [Required(ErrorMessage = "Authority is required")]
    [Url(ErrorMessage = "Authority must be a valid URL")]
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The anonymous API key for Supabase REST API authentication.
    /// This is the public anon key from your Supabase project settings.
    /// </summary>
    public string? ApiKey { get; set; }
}
