using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Enums;

namespace Shared.Users.Infrastructure.Services;

public class UserService(IUsersDbContext dbContext) : IUserService
{
    public async Task<User?> GetByExternalIdAsync(IdentityProvider provider, string externalUserId, CancellationToken ct = default)
    {
        return await dbContext.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.ExternalProviders.Any(ep => ep.Provider == provider && ep.ExternalUserId == externalUserId), ct);
    }

    public async Task<User> CreateAsync(IdentityProvider provider, string externalUserId, string email, CancellationToken ct = default)
    {
        var user = User.ProvisionNew(email, provider, externalUserId);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);

        return user;
    }
}