using Shared.Users.Domain.Aggregates;
using Shared.Users.Domain.Enums;

namespace Shared.Users.Application.Abstractions;

public interface IUserService
{
    Task<User?> GetByExternalIdAsync(
        IdentityProvider provider,
        string externalUserId,
        CancellationToken ct = default);

    Task<User> CreateAsync(
        IdentityProvider provider,
        string externalUserId,
        string email,
        CancellationToken ct = default);
}
