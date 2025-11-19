using MediatR;

namespace Shared.Users.Application.Commands;

/// <summary>
/// Command to grant a permission directly to a user (separate from role-based permissions)
/// </summary>
public record GrantPermissionToUserCommand(
    Guid UserId,
    string PermissionName,
    string? Reason = null) : IRequest;
