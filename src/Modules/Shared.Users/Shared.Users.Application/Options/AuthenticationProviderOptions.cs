using Shared.Abstractions.Options;
using Shared.Users.Domain;

namespace Shared.Users.Application.Options;

/// <summary>
/// Configuration for selecting which authentication provider to use.
/// Specifies whether to use Supabase, Clerk, or no provider.
/// </summary>
public class AuthenticationProviderOptions : IOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public static string SectionName => $"Modules__{ModuleConstants.ModuleName}__Authentication";

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
