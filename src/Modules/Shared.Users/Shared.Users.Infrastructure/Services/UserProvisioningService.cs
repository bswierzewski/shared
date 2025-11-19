using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Entities;
using Shared.Users.Domain.Enums;

namespace Shared.Users.Infrastructure.Services;

/// <summary>
/// Service responsible for Just-In-Time (JIT) user provisioning.
/// Implements upsert logic: create new or update existing users.
/// </summary>
internal class UserProvisioningService : IUserProvisioningService
{
    private readonly IUsersWriteDbContext _writeContext;

    public UserProvisioningService(IUsersWriteDbContext writeContext)
    {
        _writeContext = writeContext;
    }

    /// <summary>
    /// Creates a new user or updates an existing one (upsert operation).
    /// Flow:
    /// 1. Try to find by external provider ID
    /// 2. If not found, try to find by email (same email = link new provider)
    /// 3. If not found, create new user (JIT provisioning)
    /// 4. If found, update profile and last login
    /// </summary>
    public async Task<User> UpsertUserAsync(
        IdentityProvider provider,
        string externalUserId,
        string? email,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        // 1. Try to find by external provider ID
        var user = await _writeContext.Users
            .FirstOrDefaultAsync(u => u.ExternalProviders
                .Any(ep => ep.Provider == provider && ep.ExternalUserId == externalUserId),
            cancellationToken);

        // 2. Try to find by email if not found
        if (user == null && !string.IsNullOrEmpty(email))
        {
            user = await _writeContext.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            // Link additional provider if user exists with same email
            if (user != null)
            {
                user.LinkExternalProvider(provider, externalUserId);
            }
        }

        // 3. Create new user if not found (JIT provisioning)
        if (user == null)
        {
            if (string.IsNullOrEmpty(email))
                throw new InvalidOperationException("Email is required for new user provisioning");

            user = User.ProvisionNew(email, displayName, null, provider, externalUserId);
            _writeContext.Users.Add(user);
        }
        else
        {
            // 4. Update profile for existing user
            user.UpdateProfile(displayName, null);
        }

        await _writeContext.SaveChangesAsync(cancellationToken);
        return user;
    }
}
