using ErrorOr;
using MediatR;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Query to get all roles
/// Returns a list of all roles in the system
/// </summary>
public record GetAllRolesQuery : IRequest<ErrorOr<IReadOnlyCollection<RoleDto>>>;
