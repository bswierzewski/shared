using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Handler for GetAllPermissionsQuery
/// Retrieves all permissions from the database
/// </summary>
internal class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, ErrorOr<IReadOnlyCollection<PermissionDto>>>
{
    private readonly IUsersDbContext _context;

    public GetAllPermissionsQueryHandler(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IReadOnlyCollection<PermissionDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .Select(p => new PermissionDto(
                p.Id,
                p.Name,
                p.Description,
                p.IsActive,
                p.IsModule))
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
