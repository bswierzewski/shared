namespace Shared.Users.Domain.Enums;

/// <summary>
/// Enum representing identity providers (Auth0, Clerk, Google, etc.)
/// </summary>
public enum IdentityProvider
{
    /// <summary>
    /// Auth0 identity provider
    /// </summary>
    Auth0 = 1,

    /// <summary>
    /// Clerk identity provider
    /// </summary>
    Clerk = 2,

    /// <summary>
    /// Google identity provider
    /// </summary>
    Google = 3,

    /// <summary>
    /// Microsoft identity provider
    /// </summary>
    Microsoft = 4,

    /// <summary>
    /// Other/unknown identity provider
    /// </summary>
    Other = 5
}
