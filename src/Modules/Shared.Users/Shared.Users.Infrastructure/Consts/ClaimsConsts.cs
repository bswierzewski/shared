namespace Shared.Users.Infrastructure.Consts;

/// <summary>
/// Constants for claim type names used throughout the Users module.
/// Centralizes claim names to avoid magic strings and ensure consistency.
/// </summary>
public static class ClaimsConsts
{
    /// <summary>
    /// Claim type for user ID (maps to ClaimTypes.NameIdentifier in standard claims).
    /// Value: "user_id"
    /// </summary>
    public const string UserId = "user_id";

    /// <summary>
    /// Claim type for identity provider name.
    /// Value: "provider"
    /// Examples: "Clerk", "Supabase", etc.
    /// </summary>
    public const string Provider = "provider";

    /// <summary>
    /// Claim type for user picture/avatar URL.
    /// Value: "picture_url"
    /// </summary>
    public const string PictureUrl = "picture_url";

    /// <summary>
    /// Claim type for user permission.
    /// Value: "permission"
    /// Multiple permission claims can exist for a single user.
    /// </summary>
    public const string Permission = "permission";
}
