using Microsoft.Extensions.Options;
using Shared.Users.Domain.Enums;

namespace Shared.Infrastructure.Tests.Authentication;

/// <summary>
/// Token provider for Clerk authentication.
/// Authenticates against Clerk API using email/password credentials.
/// Fetches access token from Clerk auth endpoint.
/// </summary>
public class ClerkTokenProvider : ITokenProvider
{
    private readonly TestUserOptions _userOptions;

    /// <summary>
    /// Gets the identity provider type (Clerk).
    /// </summary>
    public IdentityProvider Provider => IdentityProvider.Clerk;

    /// <summary>
    /// Creates a new ClerkTokenProvider with test user credentials.
    /// </summary>
    /// <param name="userOptions">Test user configuration (email and password)</param>
    public ClerkTokenProvider(IOptions<TestUserOptions> userOptions)
    {
        ArgumentNullException.ThrowIfNull(userOptions, nameof(userOptions));
        ArgumentException.ThrowIfNullOrEmpty(userOptions.Value.Email, nameof(userOptions.Value.Email));
        ArgumentException.ThrowIfNullOrEmpty(userOptions.Value.Password, nameof(userOptions.Value.Password));

        _userOptions = userOptions.Value;
    }

    /// <summary>
    /// Authenticates with Clerk using configured test user credentials.
    /// Parameters are ignored - uses email and password from configuration.
    /// </summary>
    /// <param name="login">Ignored - uses email from configuration</param>
    /// <param name="password">Ignored - uses password from configuration</param>
    /// <returns>Access token from Clerk auth endpoint</returns>
    /// <exception cref="NotImplementedException">Clerk authentication integration not yet implemented</exception>
    public Task<string> GetTokenAsync(string login, string password)
    {
        // TODO: Implement Clerk authentication
        // This will authenticate with Clerk API using _userOptions.Email and _userOptions.Password
        // and return the obtained access token
        throw new NotImplementedException(
            "Clerk token provider authentication is not yet implemented. " +
            "Please implement authentication against Clerk API.");
    }
}
