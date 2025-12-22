using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Handler for assigning one or more roles to a user
/// </summary>
internal class AssignRoleToUserCommandHandler(IUsersDbContext context) : IRequestHandler<AssignRoleToUserCommand>
{
    public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        // Load user with roles navigation property
        var user = await context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"User {request.UserId} not found");

        // Load all requested roles by Names
        var roles = await context.Roles
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

        await context.SaveChangesAsync(cancellationToken);
    }
}
