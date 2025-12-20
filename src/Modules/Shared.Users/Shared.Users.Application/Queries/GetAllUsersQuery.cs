using ErrorOr;
using MediatR;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Query to get all users
/// Returns a list of all users in the system
/// </summary>
public record GetAllUsersQuery : IRequest<ErrorOr<IReadOnlyCollection<UserDto>>>;
