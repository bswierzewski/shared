using Microsoft.AspNetCore.Http;
using Shared.Abstractions.Abstractions;
using Shared.Users.Infrastructure.Consts;
using System.Security.Claims;

namespace Shared.Users.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : IUser
{
    private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    private readonly Lazy<Guid> _id = new(() =>
    {
        var value = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimsConsts.UserId);
        return Guid.TryParse(value, out var guid) ? guid : Guid.Empty;
    });

    private readonly Lazy<HashSet<string>> _roles = new(() =>
        httpContextAccessor.HttpContext?.User.FindAll(ClaimsConsts.Role)
             .Select(c => c.Value)
             .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? []);

    private readonly Lazy<HashSet<string>> _permissions = new(() =>
        httpContextAccessor.HttpContext?.User.FindAll(ClaimsConsts.Permission)
             .Select(c => c.Value)
             .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? []);

    public Guid Id => _id.Value;

    public string Email => User.FindFirstValue(ClaimsConsts.Email) ?? string.Empty;

    public IEnumerable<string> Roles => _roles.Value;

    public IEnumerable<string> Permissions => _permissions.Value;

    public bool IsInRole(string role) => _roles.Value.Contains(role);

    public bool HasPermission(string permission) => _permissions.Value.Contains(permission);
}