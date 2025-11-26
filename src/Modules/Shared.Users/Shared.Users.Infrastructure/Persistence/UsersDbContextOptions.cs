using System.ComponentModel.DataAnnotations;
using Shared.Abstractions.Options;
using Shared.Users.Domain;

namespace Shared.Users.Infrastructure.Persistence;

/// <summary>
/// Configuration options for the Users module DbContext.
/// </summary>
public class UsersDbContextOptions : IOptions
{
    /// <summary>
    /// The configuration section name for Users module database configuration.
    /// </summary>
    public static string SectionName => $"Modules:{ModuleConstants.ModuleName}";

    /// <summary>
    /// The database connection string for the Users module.
    /// </summary>
    [Required(ErrorMessage = "ConnectionString is required")]
    public string ConnectionString { get; set; } = null!;
}
