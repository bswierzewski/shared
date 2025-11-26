using Shared.Infrastructure.Tests.Builders;
using Shared.Infrastructure.Tests.Core;
using Shared.Infrastructure.Tests.Extensions.Http;
using Shared.Users.Infrastructure.Persistence;

namespace Shared.Users.Tests.EndToEnd.UseCase;

/// <summary>
/// E2E tests for Just-In-Time (JIT) user provisioning.
/// Tests the flow where users are automatically created or updated when they authenticate
/// with a new external provider or for the first time.
/// </summary>
[Collection("Users")]
public class UserProvisioningTests
{
    private readonly TestContext _context;

    public UserProvisioningTests(UsersTestFixture fixture)
    {
        _context = fixture.Context;
    }
    /// <summary>
    /// Tests that a new user is automatically provisioned (created) when they authenticate for the first time.
    /// The JitProvisioningMiddleware calls UserProvisioningService.UpsertUserAsync() which creates the user.
    /// </summary>
    [Fact]
    public async Task NewUser_ShouldBeProvisionedAutomatically()
    {
        // Reset database
        await _context.ResetDatabaseAsync();

        // Arrange
        var email = "newuser@example.com";
        var token = _context.GenerateUserToken(email);
        _context.Client.WithBearerToken(token);

        // Act - Call any endpoint that requires authentication
        var response = await _context.Client.GetAsync("/api/users");

        // Assert - Endpoint should succeed (or return 401 if no GetAll, but user is provisioned)
        // The important part is that JIT provisioning happens in the middleware
        var userExists = await _context.UserExistsAsync(email);
        userExists.Should().BeTrue();

        // Verify user was created with correct data
        var user = await _context.GetUserFromDbAsync(email);
        user.Email.Should().Be(email);
        user.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Tests that when an existing user authenticates again, they are not duplicated.
    /// The user should be found by external provider ID and updated, not created new.
    /// </summary>
    [Fact]
    public async Task ExistingUser_ShouldNotBeDuplicated()
    {
        // Arrange - Provision user on first request
        var email = "existing@example.com";
        var externalUserId = "ext-123";

        var token1 = _context.GenerateUserToken(email, externalUserId, "First Name");
        _context.Client.WithBearerToken(token1);

        // Act - First request
        await _context.Client.GetAsync("/api/users");
        var firstUser = await _context.GetUserFromDbAsync(email);
        var firstUserId = firstUser.Id;

        // Arrange - Second request with same email and external ID
        var token2 = _context.GenerateUserToken(email, externalUserId, "Updated Name");
        _context.Client.WithBearerToken(token2);

        // Act - Second request
        await _context.Client.GetAsync("/api/users");

        // Assert - Same user should be updated, not duplicated
        var secondUser = await _context.GetUserFromDbAsync(email);
        secondUser.Id.Should().Be(firstUserId);

        // Verify only one user with this email exists
        var db = _context.GetRequiredService<UsersDbContext>();
        var allWithEmail = await db.Users
            .Where(u => u.Email == email)
            .ToListAsync();
        allWithEmail.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that a user's last login is updated when they authenticate again.
    /// LastLoginAt should be updated on each authentication.
    /// </summary>
    [Fact]
    public async Task ExistingUser_LastLoginShouldBeUpdated()
    {
        // Arrange - Create user
        var email = "login@example.com";
        var token1 = _context.GenerateUserToken(email);
        _context.Client.WithBearerToken(token1);
        await _context.Client.GetAsync("/api/users");

        var user1 = await _context.GetUserFromDbAsync(email);
        user1.LastLoginAt.Should().NotBeNull();
        var firstLogin = user1.LastLoginAt!.Value;

        // Act - Wait a bit and authenticate again
        await Task.Delay(100);
        var token2 = _context.GenerateUserToken(email);
        _context.Client.WithBearerToken(token2);
        await _context.Client.GetAsync("/api/users");

        // Assert - Last login should be updated
        var user2 = await _context.GetUserFromDbAsync(email);
        user2.LastLoginAt.Should().NotBeNull();
        user2.LastLoginAt!.Value.Should().BeAfter(firstLogin);
    }

    /// <summary>
    /// Tests that expired tokens are still processed for JIT provisioning.
    /// Test auth disables lifetime validation, so expired tokens should work.
    /// </summary>
    [Fact]
    public async Task ExpiredToken_ShouldStillProvisionUser()
    {
        // Arrange - Create token with expiry in the past
        var email = "expired@example.com";
        var token = new JwtTokenBuilder()
            .WithEmail(email)
            .WithSubject(Guid.NewGuid().ToString())
            .WithExpiration(DateTime.UtcNow.AddHours(-1)) // Expired 1 hour ago
            .Build();

        _context.Client.WithBearerToken(token);

        // Act - Make authenticated request with expired token
        await _context.Client.GetAsync("/api/users");

        // Assert - User should still be provisioned despite expired token
        var userExists = await _context.UserExistsAsync(email);
        userExists.Should().BeTrue();
    }

    /// <summary>
    /// Tests that multiple external providers can be linked to the same user.
    /// When a user authenticates with a different provider but same email, they should be linked.
    /// </summary>
    [Fact]
    public async Task SameEmail_DifferentProvider_ShouldLinkProviders()
    {
        // Arrange - Create user with first provider
        var email = "multiauth@example.com";
        var supabaseId = "supabase-123";

        var token1 = _context.GenerateUserToken(email, supabaseId);
        _context.Client.WithBearerToken(token1);
        await _context.Client.GetAsync("/api/users");

        var user1 = await _context.GetUserFromDbAsync(email);
        var providers1 = await _context.GetRequiredService<UsersDbContext>()
            .Users
            .Where(u => u.Id == user1.Id)
            .Select(u => u.ExternalProviders)
            .FirstAsync();

        providers1.Should().HaveCount(1);

        // Act - Authenticate with different external ID (simulating different provider)
        var clerkId = "clerk-456";
        var token2 = _context.GenerateUserToken(email, clerkId);
        _context.Client.WithBearerToken(token2);
        await _context.Client.GetAsync("/api/users");

        // Assert - Same user should now have two external providers linked
        var user2 = await _context.GetUserFromDbAsync(email);
        user2.Id.Should().Be(user1.Id);

        var providersAfter = await _context.GetRequiredService<UsersDbContext>()
            .Users
            .Where(u => u.Id == user2.Id)
            .Select(u => u.ExternalProviders)
            .FirstAsync();

        providersAfter.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that users are created as active by default.
    /// </summary>
    [Fact]
    public async Task NewUser_ShouldBeActiveByDefault()
    {
        // Arrange
        var email = "active@example.com";
        var token = _context.GenerateUserToken(email);
        _context.Client.WithBearerToken(token);

        // Act
        await _context.Client.GetAsync("/api/users");

        // Assert
        var user = await _context.GetUserFromDbAsync(email);
        user.IsActive.Should().BeTrue();
    }

}
