using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Handler for removing a role from a user
/// </summary>
internal class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand>
{
    private readonly IUsersDbContext _context;

    public RemoveRoleFromUserCommandHandler(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        // Load user with roles navigation property
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"User {request.UserId} not found");

        // Find the role by name
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken);

        // If role doesn't exist, operation is idempotent (no-op)
        if (role == null)
            return;

        // Remove role from user
        user.RemoveRole(role);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
