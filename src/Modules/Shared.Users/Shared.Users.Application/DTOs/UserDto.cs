namespace Shared.Users.Application.DTOs;

/// <summary>
/// Data Transfer Object for User
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    bool IsActive,
    DateTimeOffset? LastLoginAt,
    IReadOnlyCollection<ExternalProviderDto> ExternalProviders,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions);

/// <summary>
/// Data Transfer Object for ExternalProvider
/// </summary>
public record ExternalProviderDto(
    string Provider,
    string ExternalUserId,
    DateTimeOffset AddedAt);

/// <summary>
/// Data Transfer Object for Role
/// </summary>
public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool IsModule);

/// <summary>
/// Data Transfer Object for Permission
/// </summary>
public record PermissionDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool IsModule);
