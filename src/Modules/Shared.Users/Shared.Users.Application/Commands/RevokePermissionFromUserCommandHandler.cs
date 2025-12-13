using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Handler for revoking a directly granted permission from a user
/// </summary>
internal class RevokePermissionFromUserCommandHandler : IRequestHandler<RevokePermissionFromUserCommand>
{
    private readonly IUsersDbContext _context;

    public RevokePermissionFromUserCommandHandler(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RevokePermissionFromUserCommand request, CancellationToken cancellationToken)
    {
        // Load user with permissions navigation property
        var user = await _context.Users
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"User {request.UserId} not found");

        // Find the permission by name
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == request.PermissionName, cancellationToken);

        // If permission doesn't exist, operation is idempotent (no-op)
        if (permission == null)
            return;

        // Revoke permission from user
        user.RevokePermission(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
