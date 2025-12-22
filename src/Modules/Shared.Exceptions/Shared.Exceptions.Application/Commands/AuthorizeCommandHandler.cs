using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Handler for RoleProtectedCommand that returns a successful response.
/// This handler is only reachable if the user has the "admin" role.
/// </summary>
public class AuthorizeCommandHandler : IRequestHandler<AuthorizeCommand, ErrorOr<string>>
{
    /// <summary>
    /// Handles the RoleProtectedCommand by returning a successful response.
    /// </summary>
    public Task<ErrorOr<string>> Handle(AuthorizeCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult<ErrorOr<string>>("Permission protected response - user has permissions");
    }
}
