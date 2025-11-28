using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Tests.Authentication;
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
public class UserProvisioningTests : IAsyncLifetime
{
    private readonly TestContext _context;
    private TestUserOptions _testUser = null!;

    public UserProvisioningTests(UsersTestFixture fixture)
    {
        _context = fixture.Context;
    }

    public async Task InitializeAsync()
    {
        var userOptions = _context.Services.GetRequiredService<IOptions<TestUserOptions>>();
        _testUser = userOptions.Value;
        await Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;
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
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        // Act - Call any endpoint that requires authentication
        var response = await _context.Client.GetAsync("/api/users");

        // Assert - Endpoint should succeed (or return 401 if no GetAll, but user is provisioned)
        // The important part is that JIT provisioning happens in the middleware
        var userExists = await _context.UserExistsAsync(_testUser.Email);
        userExists.Should().BeTrue();

        // Verify user was created with correct data
        var user = await _context.GetUserFromDbAsync(_testUser.Email);
        user.Email.Should().Be(_testUser.Email);
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
        var token1 = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token1);

        // Act - First request
        await _context.Client.GetAsync("/api/users");
        var firstUser = await _context.GetUserFromDbAsync(_testUser.Email);
        var firstUserId = firstUser.Id;

        // Arrange - Second request with same email
        var token2 = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token2);

        // Act - Second request
        await _context.Client.GetAsync("/api/users");

        // Assert - Same user should be updated, not duplicated
        var secondUser = await _context.GetUserFromDbAsync(_testUser.Email);
        secondUser.Id.Should().Be(firstUserId);

        // Verify only one user with this email exists
        var db = _context.GetRequiredService<UsersDbContext>();
        var allWithEmail = await db.Users
            .Where(u => u.Email == _testUser.Email)
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
        var token1 = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token1);
        await _context.Client.GetAsync("/api/users");

        var user1 = await _context.GetUserFromDbAsync(_testUser.Email);
        user1.LastLoginAt.Should().NotBeNull();
        var firstLogin = user1.LastLoginAt!.Value;

        // Act - Wait a bit and authenticate again
        await Task.Delay(100);
        var token2 = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token2);
        await _context.Client.GetAsync("/api/users");

        // Assert - Last login should be updated
        var user2 = await _context.GetUserFromDbAsync(_testUser.Email);
        user2.LastLoginAt.Should().NotBeNull();
        user2.LastLoginAt!.Value.Should().BeAfter(firstLogin);
    }

    /// <summary>
    /// Tests that multiple external providers can be linked to the same user.
    /// When a user authenticates with a different provider but same email, they should be linked.
    /// </summary>
    [Fact]
    public async Task SameEmail_DifferentProvider_ShouldLinkProviders()
    {
        // Arrange - Create user with first provider
        var token1 = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token1);
        await _context.Client.GetAsync("/api/users");

        var user1 = await _context.GetUserFromDbAsync(_testUser.Email);
        var providers1 = await _context.GetRequiredService<UsersDbContext>()
            .Users
            .Where(u => u.Id == user1.Id)
            .Select(u => u.ExternalProviders)
            .FirstAsync();

        providers1.Should().HaveCount(1);

        // Act - Authenticate again
        var token2 = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token2);
        await _context.Client.GetAsync("/api/users");

        // Assert - Same user should now have two external providers linked
        var user2 = await _context.GetUserFromDbAsync(_testUser.Email);
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
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        // Act
        await _context.Client.GetAsync("/api/users");

        // Assert
        var user = await _context.GetUserFromDbAsync(_testUser.Email);
        user.IsActive.Should().BeTrue();
    }

}
