using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Enums;

namespace Shared.Users.Infrastructure.Services;

/// <summary>
/// Service responsible for Just-In-Time (JIT) user provisioning.
/// Implements upsert logic: create new or update existing users.
/// </summary>
internal class UserProvisioningService : IUserProvisioningService
{
    private readonly IUsersDbContext _context;

    public UserProvisioningService(IUsersDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new user or updates an existing one (upsert operation).
    /// Flow:
    /// 1. Try to find by external provider ID
    /// 2. If not found, try to find by email (same email = link new provider and update profile)
    /// 3. If not found, create new user (JIT provisioning)
    /// 4. If found by provider, update profile and last login
    ///
    /// Note: Picture URL is obtained from the JWT token and is not stored in the domain.
    /// </summary>
    public async Task<User> UpsertUserAsync(
        IdentityProvider provider,
        string externalUserId,
        string? email,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        // 1. Try to find by external provider ID
        var user = await _context.Users
            .Include(u => u.ExternalProviders)
            .FirstOrDefaultAsync(u => u.ExternalProviders
                .Any(ep => ep.Provider == provider && ep.ExternalUserId == externalUserId),
            cancellationToken);

        // 2. Try to find by email if not found by provider
        if (user == null && !string.IsNullOrEmpty(email))
        {
            user = await _context.Users
                .Include(u => u.ExternalProviders)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            // Link additional provider if user exists with same email
            if (user != null)
            {
                var newProvider = user.LinkExternalProvider(provider, externalUserId);
                // Explicitly add new provider to DbContext so it gets saved
                if (newProvider != null)
                {
                    _context.ExternalProviders.Add(newProvider);
                }
            }
        }

        // 3. Create new user if not found (JIT provisioning)
        if (user == null)
        {
            if (string.IsNullOrEmpty(email))
                throw new InvalidOperationException("Email is required for new user provisioning");

            user = User.ProvisionNew(email, provider, externalUserId);
            _context.Users.Add(user);
        }
        else
        {
            // 4. Update last login for existing user (found by provider OR by email)
            user.UpdateLastLogin();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }
}
