using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Handler for RoleProtectedCommand that returns a successful response.
/// This handler is only reachable if the user has the "admin" role.
/// </summary>
public class RoleProtectedCommandHandler : IRequestHandler<RoleProtectedCommand, ErrorOr<string>>
{
    /// <summary>
    /// Handles the RoleProtectedCommand by returning a successful response.
    /// </summary>
    public Task<ErrorOr<string>> Handle(RoleProtectedCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult<ErrorOr<string>>("Role protected response - user has admin role");
    }
}
