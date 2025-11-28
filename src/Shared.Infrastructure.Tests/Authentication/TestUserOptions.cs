using Shared.Abstractions.Options;

namespace Shared.Infrastructure.Tests.Authentication;

/// <summary>
/// Configuration options for test user credentials.
/// Defines the email and password for test users across all authentication providers.
/// </summary>
public class TestUserOptions : IOptions
{
    /// <summary>
    /// Configuration section name in appsettings.
    /// </summary>
    public static string SectionName => "TestUser";

    /// <summary>
    /// Gets or sets the email of the test user.
    /// Example: testuser@example.com
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the test user.
    /// Used across all authentication providers.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
