using ErrorOr;
using MediatR;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Query to get a user by ID
/// Returns Error.NotFound if user not found
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IRequest<ErrorOr<UserDto>>;
