using MediatR;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Command to assign one or more roles to a user
/// </summary>
public record AssignRoleToUserCommand(
    Guid UserId,
    IEnumerable<string> RoleIds) : IRequest;
