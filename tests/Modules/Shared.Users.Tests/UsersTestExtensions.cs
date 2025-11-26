using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Tests.Core;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Infrastructure.Persistence;
using Shared.Users.Tests.Authentication;

namespace Shared.Users.Tests;

/// <summary>
/// Extension methods for Users module tests.
/// Provides domain-specific test helpers as extension methods on TestContext.
/// </summary>
public static class UsersTestExtensions
{
    /// <summary>
    /// Generates a test JWT token with specified claims.
    /// Uses JwtTokenBuilder to create a valid JWT structure.
    /// </summary>
    /// <param name="context">The test context.</param>
    /// <param name="email">Email claim (required)</param>
    /// <param name="userId">Subject (sub) claim - external user ID. Defaults to random GUID.</param>
    /// <param name="displayName">Display name claim. Optional.</param>
    /// <param name="additionalClaims">Additional custom claims. Optional.</param>
    /// <returns>A valid JWT token string</returns>
    public static string GenerateUserToken(
        this TestContext context,
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
    /// <param name="context">The test context.</param>
    /// <param name="email">User email to search for</param>
    /// <returns>The user entity or throws if not found</returns>
    public static async Task<User> GetUserFromDbAsync(
        this TestContext context,
        string email)
    {
        var db = context.GetRequiredService<UsersDbContext>();
        return await db.Users
            .AsNoTracking()
            .FirstAsync(u => u.Email == email);
    }

    /// <summary>
    /// Retrieves a user with their roles loaded.
    /// </summary>
    public static async Task<User> GetUserWithRolesFromDbAsync(
        this TestContext context,
        string email)
    {
        var db = context.GetRequiredService<UsersDbContext>();
        return await db.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstAsync(u => u.Email == email);
    }

    /// <summary>
    /// Retrieves a user with their permissions loaded.
    /// </summary>
    public static async Task<User> GetUserWithPermissionsFromDbAsync(
        this TestContext context,
        string email)
    {
        var db = context.GetRequiredService<UsersDbContext>();
        return await db.Users
            .AsNoTracking()
            .Include(u => u.Permissions)
            .FirstAsync(u => u.Email == email);
    }

    /// <summary>
    /// Checks if a user exists in the database.
    /// </summary>
    public static async Task<bool> UserExistsAsync(
        this TestContext context,
        string email)
    {
        var db = context.GetRequiredService<UsersDbContext>();
        return await db.Users.AnyAsync(u => u.Email == email);
    }
}
