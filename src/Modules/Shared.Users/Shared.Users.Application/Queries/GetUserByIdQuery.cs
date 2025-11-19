using MediatR;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Query to get a user by ID
/// Returns null if user not found
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
