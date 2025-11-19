using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Handler for removing a role from a user
/// </summary>
internal class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand>
{
    private readonly IUsersWriteDbContext _writeContext;

    public RemoveRoleFromUserCommandHandler(IUsersWriteDbContext writeContext)
    {
        _writeContext = writeContext;
    }

    public async Task Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        // Load user with roles navigation property
        var user = await _writeContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"User {request.UserId} not found");

        // Find the role by name
        var role = await _writeContext.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken);

        if (role == null)
            throw new InvalidOperationException($"Role '{request.RoleName}' not found");

        // Remove role from user
        user.RemoveRole(role);
        await _writeContext.SaveChangesAsync(cancellationToken);
    }
}
