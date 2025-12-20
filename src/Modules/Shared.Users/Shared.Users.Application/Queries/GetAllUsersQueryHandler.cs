using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Handler for GetAllUsersQuery
/// Retrieves all users from the database
/// </summary>
internal class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ErrorOr<IReadOnlyCollection<UserDto>>>
{
    private readonly IUsersDbContext _context;

    public GetAllUsersQueryHandler(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IReadOnlyCollection<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.ExternalProviders)
            .Include(u => u.Roles)
            .Include(u => u.Permissions)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.IsActive,
                u.LastLoginAt,
                u.ExternalProviders
                    .Select(ep => new ExternalProviderDto(
                        ep.Provider.ToString(),
                        ep.ExternalUserId,
                        ep.AddedAt))
                    .ToList(),
                u.Roles
                    .Select(r => new RoleDto(
                        r.Id,
                        r.Name,
                        r.Description,
                        r.IsActive,
                        r.IsModule))
                    .ToList(),
                u.Permissions
                    .Select(p => new PermissionDto(
                        p.Id,
                        p.Name,
                        p.Description,
                        p.IsActive,
                        p.IsModule))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return users;
    }
}
