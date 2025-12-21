using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Handler for assigning one or more roles to a user
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

        // Load all requested roles by Names
        var roles = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Name))
            .ToListAsync(cancellationToken);

        // Verify all roles exist
        if (roles.Count != request.RoleIds.Count())
        {
            var missingIds = request.RoleIds.Except(roles.Select(r => r.Name));
            throw new InvalidOperationException($"Roles not found: {string.Join(", ", missingIds)}");
        }

        // Assign all roles to the user
        foreach (var role in roles)        
            user.AssignRole(role);        

        await _context.SaveChangesAsync(cancellationToken);
    }
}
