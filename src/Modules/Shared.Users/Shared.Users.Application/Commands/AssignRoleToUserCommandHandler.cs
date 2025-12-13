using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Handler for assigning a role to a user
/// </summary>
internal class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IUsersDbContext _context;

    public AssignRoleToUserCommandHandler(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
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

        if (role == null)
            throw new InvalidOperationException($"Role '{request.RoleName}' not found");

        // Assign role to user
        user.AssignRole(role);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
