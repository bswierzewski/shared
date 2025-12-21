using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
internal class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ErrorOr<UserDto>>
{
    private readonly IUsersDbContext _context;

    public GetUserByIdQueryHandler(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.ExternalProviders)
            .Include(u => u.Roles)
            .Where(u => u.Id == request.UserId)
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
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User.NotFound", "User not found");
        }

        return user;
    }
}
