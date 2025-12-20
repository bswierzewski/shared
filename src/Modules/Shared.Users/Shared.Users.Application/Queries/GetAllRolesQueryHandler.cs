using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Handler for GetAllRolesQuery
/// Retrieves all roles from the database
/// </summary>
internal class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, ErrorOr<IReadOnlyCollection<RoleDto>>>
{
    private readonly IUsersDbContext _context;

    public GetAllRolesQueryHandler(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IReadOnlyCollection<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsActive,
                r.IsModule))
            .ToListAsync(cancellationToken);

        return roles;
    }
}
