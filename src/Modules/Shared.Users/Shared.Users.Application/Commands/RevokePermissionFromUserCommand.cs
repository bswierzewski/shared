using MediatR;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Command to revoke a directly granted permission from a user
/// </summary>
public record RevokePermissionFromUserCommand(
    Guid UserId,
    string PermissionName) : IRequest;
