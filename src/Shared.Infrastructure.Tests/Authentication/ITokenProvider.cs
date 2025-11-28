using Shared.Users.Domain.Enums;

namespace Shared.Infrastructure.Tests.Authentication;

/// <summary>
/// Interface for providing authentication tokens for different auth providers.
/// Implementations handle provider-specific token acquisition logic (Clerk, Supabase, Test).
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Gets the identity provider type (Clerk, Supabase, Test, etc).
    /// </summary>
    IdentityProvider Provider { get; }

    /// <summary>
    /// Obtains an authentication token for the given credentials.
    /// Implementation varies by provider:
    /// - Clerk: Returns pre-generated hardcoded token for the user
    /// - Supabase: Authenticates against Supabase auth endpoint and returns access token
    /// - Test: Generates JWT token in-memory for testing
    /// </summary>
    /// <param name="login">User login/email address</param>
    /// <param name="password">User password (may be ignored by some providers)</param>
    /// <returns>JWT token string that can be used in Authorization header</returns>
    /// <exception cref="InvalidOperationException">Thrown if credentials are invalid or token cannot be obtained</exception>
    Task<string> GetTokenAsync(string login, string password);
}
