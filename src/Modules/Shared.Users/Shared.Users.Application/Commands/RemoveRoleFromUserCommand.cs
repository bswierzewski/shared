using MediatR;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Command to remove a role from a user
/// </summary>
public record RemoveRoleFromUserCommand(
    Guid UserId,
    string RoleName) : IRequest;
