using MediatR;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Command to assign a role to a user
/// </summary>
public record AssignRoleToUserCommand(
    Guid UserId,
    string RoleName) : IRequest;
