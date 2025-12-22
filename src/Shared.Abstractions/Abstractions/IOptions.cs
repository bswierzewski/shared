namespace Shared.Abstractions.Abstractions;

/// <summary>
/// Marker interface for options classes used throughout the application.
/// Implements this interface to mark a class as an options/configuration class
/// that should be loaded from application configuration using Microsoft.Extensions.Options.
///
/// Usage:
/// - Create a class that implements this interface
/// - Define the static SectionName property to specify the configuration section
/// - Configure it with IConfiguration in dependency injection using Configure&lt;TOptions&gt;(configuration.GetSection(TOptions.SectionName))
/// - Use Microsoft.Extensions.Options.IOptions&lt;TOptions&gt; to inject the configured options
///
/// Example:
/// public class MyModuleOptions : IOptions
/// {
///     public static string SectionName => "MyModule";
///
///     [Required]
///     public string ConnectionString { get; set; } = null!;
///
///     public int Timeout { get; set; } = 30;
/// }
///
/// In your module registration:
/// services.Configure&lt;MyModuleOptions&gt;(configuration.GetSection(MyModuleOptions.SectionName));
/// </summary>
public interface IOptions
{
    /// <summary>
    /// Gets the configuration section name for binding this options class.
    /// Each options class must define this static property to specify where in the configuration
    /// hierarchy this options class should be loaded from.
    ///
    /// Example: "Users:Database" or "Auth:Jwt" or "Email:Smtp"
    /// </summary>
    static abstract string SectionName { get; }
}
