using Shared.Infrastructure.Tests.Core;
using Shared.Infrastructure.Tests.Extensions.Http;
using Shared.Users.Infrastructure.Persistence;
using Shared.Users.Tests.EndToEnd.Extensions;

namespace Shared.Users.Tests.EndToEnd.UseCase;

/// <summary>
/// E2E tests for Just-In-Time (JIT) user provisioning.
/// Tests the flow where users are automatically created or updated when they authenticate
/// with a new external provider or for the first time.
/// </summary>
[Collection("Users")]
public class UserProvisioningTests(UsersSharedFixture fixture) : IAsyncLifetime
{
    private TestContext _context = null!;

    public async Task InitializeAsync()
    {
        _context = await TestContext.CreateBuilder<Program>()
            .WithContainer(fixture.Container)
            .BuildAsync();

        await _context.ResetDatabaseAsync();

        // Get token using fixture's provider (has built-in cache)
        var token = await fixture.TokenProvider.GetTokenAsync(fixture.TestUser.Email, fixture.TestUser.Password);
        _context.Client.WithBearerToken(token);
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

        // Act - Call any endpoint that requires authentication
        var response = await _context.Client.GetAsync("/api/users");

        // Assert - Endpoint should succeed (or return 401 if no GetAll, but user is provisioned)
        // The important part is that JIT provisioning happens in the middleware
        var userExists = await _context.UserExistsAsync(fixture.TestUser.Email);
        userExists.Should().BeTrue();

        // Verify user was created with correct data
        var user = await _context.GetUserFromDbAsync(fixture.TestUser.Email);
        user.Email.Should().Be(fixture.TestUser.Email);
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
        // Act - First request
        await _context.Client.GetAsync("/api/users");
        var firstUser = await _context.GetUserFromDbAsync(fixture.TestUser.Email);
        var firstUserId = firstUser.Id;

        // Act - Second request with same user
        await _context.Client.GetAsync("/api/users");

        // Assert - Same user should be updated, not duplicated
        var secondUser = await _context.GetUserFromDbAsync(fixture.TestUser.Email);
        secondUser.Id.Should().Be(firstUserId);

        // Verify only one user with this email exists
        var db = _context.GetRequiredService<UsersDbContext>();
        var allWithEmail = await db.Users
            .Where(u => u.Email == fixture.TestUser.Email)
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
        await _context.Client.GetAsync("/api/users");

        var user1 = await _context.GetUserFromDbAsync(fixture.TestUser.Email);
        user1.LastLoginAt.Should().NotBeNull();
        var firstLogin = user1.LastLoginAt!.Value;

        // Act - Wait a bit and authenticate again
        await Task.Delay(100);
        await _context.Client.GetAsync("/api/users");

        // Assert - Last login should be updated
        var user2 = await _context.GetUserFromDbAsync(fixture.TestUser.Email);
        user2.LastLoginAt.Should().NotBeNull();
        user2.LastLoginAt!.Value.Should().BeAfter(firstLogin);
    }

    /// <summary>
    /// Tests that when a user authenticates multiple times with the same provider and email,
    /// they are not duplicated and maintain the same external provider count.
    /// </summary>
    [Fact]
    public async Task SameEmail_SameProvider_ShouldNotDuplicate()
    {
        // Arrange - Create user
        await _context.Client.GetAsync("/api/users");

        var user1 = await _context.GetUserFromDbAsync(fixture.TestUser.Email);
        var providers1 = await _context.GetRequiredService<UsersDbContext>()
            .Users
            .Where(u => u.Id == user1.Id)
            .Select(u => u.ExternalProviders)
            .FirstAsync();

        providers1.Should().HaveCount(1);

        // Act - Authenticate again with same credentials
        await _context.Client.GetAsync("/api/users");

        // Assert - Same user should still have only one provider (no duplication)
        var user2 = await _context.GetUserFromDbAsync(fixture.TestUser.Email);
        user2.Id.Should().Be(user1.Id);

        var providersAfter = await _context.GetRequiredService<UsersDbContext>()
            .Users
            .Where(u => u.Id == user2.Id)
            .Select(u => u.ExternalProviders)
            .FirstAsync();

        providersAfter.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that users are created as active by default.
    /// </summary>
    [Fact]
    public async Task NewUser_ShouldBeActiveByDefault()
    {
        // Act
        await _context.Client.GetAsync("/api/users");

        // Assert
        var user = await _context.GetUserFromDbAsync(fixture.TestUser.Email);
        user.IsActive.Should().BeTrue();
    }
}
