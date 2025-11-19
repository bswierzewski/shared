using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Handler for granting a permission directly to a user
/// </summary>
internal class GrantPermissionToUserCommandHandler : IRequestHandler<GrantPermissionToUserCommand>
{
    private readonly IUsersWriteDbContext _writeContext;

    public GrantPermissionToUserCommandHandler(IUsersWriteDbContext writeContext)
    {
        _writeContext = writeContext;
    }

    public async Task Handle(GrantPermissionToUserCommand request, CancellationToken cancellationToken)
    {
        // Load user with permissions navigation property
        var user = await _writeContext.Users
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"User {request.UserId} not found");

        // Find the permission by name
        var permission = await _writeContext.Permissions
            .FirstOrDefaultAsync(p => p.Name == request.PermissionName, cancellationToken);

        if (permission == null)
            throw new InvalidOperationException($"Permission '{request.PermissionName}' not found");

        // Grant permission to user
        user.GrantPermission(permission);
        await _writeContext.SaveChangesAsync(cancellationToken);
    }
}
