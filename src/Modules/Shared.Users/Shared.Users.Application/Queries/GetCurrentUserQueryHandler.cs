using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Users.Application.Abstractions;
using Shared.Users.Application.DTOs;
using System.Security.Claims;

namespace Shared.Users.Application.Queries;

/// <summary>
/// Handler for GetCurrentUserQuery
/// Retrieves the currently authenticated user from the database
/// </summary>
internal class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ErrorOr<UserDto>>
{
    private readonly IUsersDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetCurrentUserQueryHandler(IUsersDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Get the user ID from the claims (sub claim contains the user ID from Supabase)
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Error.NotFound("User.NotAuthenticated", "User is not authenticated");
        }

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.ExternalProviders)
            .Include(u => u.Roles)
            .Where(u => u.Id == userId)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.IsActive,
                u.LastLoginAt,
                u.ExternalProviders
                    .Select(ep => new ExternalProviderDto(
                        ep.Provider.ToString(),
                        ep.ExternalUserId,
                        ep.AddedAt))
                    .ToList(),
                u.Roles
                    .Select(r => new RoleDto(
                        r.Name,
                        r.Description,
                        r.IsActive,
                        r.IsModule,
                        r.ModuleName))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User.NotFound", "User not found");
        }

        return user;
    }
}
