using ErrorOr;
using MediatR;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Query to get all permissions
/// Returns a list of all permissions in the system
/// </summary>
public record GetAllPermissionsQuery : IRequest<ErrorOr<IReadOnlyCollection<PermissionDto>>>;
