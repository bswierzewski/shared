using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
internal class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUsersReadDbContext _readContext;

    public GetUserByIdQueryHandler(IUsersReadDbContext readContext)
    {
        _readContext = readContext;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _readContext.Users
            .Include(u => u.ExternalProviders)
            .Include(u => u.Roles)
            .Include(u => u.Permissions)
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.DisplayName,
                u.PictureUrl,
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
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}
