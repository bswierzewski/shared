using Shared.Users.Infrastructure.Persistence;
using Shared.Users.Tests.Authentication;

namespace Shared.Users.Tests;

/// <summary>
/// Base class for Users module E2E tests.
/// Provides common test utilities and JWT token generation.
/// Inherits from TestBase to get HTTP client, service resolution, and database reset functionality.
/// </summary>
public abstract class UsersTestBase(UsersWebApplicationFactory factory) : TestBase(factory)
{
    /// <summary>
    /// Generates a test JWT token with specified claims.
    /// Uses JwtTokenBuilder to create a valid JWT structure that will be parsed by JwtSecurityTokenHandler.
    /// </summary>
    /// <param name="email">Email claim (required)</param>
    /// <param name="userId">Subject (sub) claim - external user ID from auth provider. Defaults to random GUID.</param>
    /// <param name="displayName">Display name claim. Optional.</param>
    /// <param name="additionalClaims">Additional custom claims. Optional.</param>
    /// <returns>A valid JWT token string</returns>
    protected string GenerateToken(
        string email,
        string? userId = null,
        string? displayName = null,
        Dictionary<string, string>? additionalClaims = null)
    {
        var builder = new JwtTokenBuilder()
            .WithEmail(email)
            .WithSubject(userId ?? Guid.NewGuid().ToString());

        if (!string.IsNullOrEmpty(displayName))
            builder.WithDisplayName(displayName);

        if (additionalClaims != null)
            foreach (var claim in additionalClaims)
                builder.WithClaim(claim.Key, claim.Value);

        return builder.Build();
    }

    /// <summary>
    /// Retrieves a user from the database by email.
    /// Useful for assertions after endpoint calls.
    /// </summary>
    /// <param name="email">User email to search for</param>
    /// <returns>The user entity or throws if not found</returns>
    protected async Task<Domain.Aggregates.User> GetUserFromDbAsync(string email)
    {
        var db = Resolve<UsersDbContext>();
        return await db.Users
            .AsNoTracking()
            .FirstAsync(u => u.Email == email);
    }

    /// <summary>
    /// Retrieves a user with their roles loaded.
    /// </summary>
    protected async Task<Domain.Aggregates.User> GetUserWithRolesFromDbAsync(string email)
    {
        var db = Resolve<UsersDbContext>();
        return await db.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstAsync(u => u.Email == email);
    }

    /// <summary>
    /// Retrieves a user with their permissions loaded.
    /// </summary>
    protected async Task<Domain.Aggregates.User> GetUserWithPermissionsFromDbAsync(string email)
    {
        var db = Resolve<UsersDbContext>();
        return await db.Users
            .AsNoTracking()
            .Include(u => u.Permissions)
            .FirstAsync(u => u.Email == email);
    }

    /// <summary>
    /// Checks if a user exists in the database.
    /// </summary>
    protected async Task<bool> UserExistsAsync(string email)
    {
        var db = Resolve<UsersDbContext>();
        return await db.Users.AnyAsync(u => u.Email == email);
    }
}
