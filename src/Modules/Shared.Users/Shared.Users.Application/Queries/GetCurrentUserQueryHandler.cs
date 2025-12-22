using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Abstractions;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Handler for GetCurrentUserQuery
/// Retrieves the currently authenticated user from the database
/// </summary>
internal class GetCurrentUserQueryHandler(IUsersDbContext context, IUser user) : IRequestHandler<GetCurrentUserQuery, ErrorOr<UserDto>>
{
    public async Task<ErrorOr<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userDto = await context.Users
            .AsNoTracking()
            .Include(u => u.ExternalProviders)
            .Include(u => u.Roles)
            .Where(u => u.Id == user.Id)
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
                        r.Name,
                        r.Description,
                        r.IsActive,
                        r.IsModule,
                        r.ModuleName))
                    .ToList(),
                u.Roles
                    .SelectMany(r => r.Permissions)
                        .Select(p => new PermissionDto(
                            p.Name,
                            p.Description,
                            p.IsActive,
                            p.IsModule,
                            p.ModuleName)).ToList())
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (userDto == null)
            return Error.NotFound("User.NotFound", "User not found");

        return userDto;
    }
}
