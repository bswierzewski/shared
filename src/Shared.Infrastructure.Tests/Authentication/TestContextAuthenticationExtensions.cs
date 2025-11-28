using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Tests.Core;
using Shared.Users.Domain.Enums;

namespace Shared.Infrastructure.Tests.Authentication;

/// <summary>
/// Extension methods for TestContext authentication.
/// Provides convenience methods for acquiring tokens from the configured token provider.
/// </summary>
public static class TestContextAuthenticationExtensions
{
    /// <summary>
    /// Gets an authentication token for the specified credentials.
    /// Delegates to the configured ITokenProvider (Clerk, Supabase, or Test).
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="login">User login/email address</param>
    /// <param name="password">User password (may be ignored by some providers)</param>
    /// <returns>JWT token string that can be used in Authorization header</returns>
    /// <exception cref="InvalidOperationException">Thrown if token provider is not configured or token acquisition fails</exception>
    public static async Task<string> GetTokenAsync(
        this TestContext context,
        string login,
        string password)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentException.ThrowIfNullOrEmpty(login, nameof(login));
        ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));

        return await context.TokenProvider.GetTokenAsync(login, password);
    }

    /// <summary>
    /// Gets the type of the currently configured token provider.
    /// Useful for logging or conditional test logic.
    /// </summary>
    /// <param name="context">The test context</param>
    /// <returns>Provider type (Clerk, Supabase, Test, etc.)</returns>
    public static IdentityProvider GetTokenProvider(this TestContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        return context.TokenProvider.Provider;
    }
}
