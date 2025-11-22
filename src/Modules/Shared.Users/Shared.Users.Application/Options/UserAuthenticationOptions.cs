using Shared.Abstractions.Options;

namespace Shared.Users.Application.Options;

/// <summary>
/// Main authentication configuration for the Users module.
/// Specifies which authentication provider to use (Supabase, Clerk, etc.)
/// </summary>
public class UserAuthenticationOptions : IOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public static string SectionName => "Authentication";

    /// <summary>
    /// The active authentication provider
    /// </summary>
    public AuthenticationProvider Provider { get; set; } = AuthenticationProvider.None;
}

/// <summary>
/// Supported authentication providers
/// </summary>
public enum AuthenticationProvider
{
    /// <summary>
    /// No authentication provider configured
    /// </summary>
    None = 0,

    /// <summary>
    /// Supabase authentication provider
    /// </summary>
    Supabase = 1,

    /// <summary>
    /// Clerk authentication provider
    /// </summary>
    Clerk = 2
}
