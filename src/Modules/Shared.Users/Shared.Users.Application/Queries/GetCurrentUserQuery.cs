using ErrorOr;
using MediatR;
using Shared.Users.Application.DTOs;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Query to get the currently authenticated user
/// Returns NotFound error if user not found or not authenticated
/// </summary>
public record GetCurrentUserQuery : IRequest<ErrorOr<UserDto>>;
