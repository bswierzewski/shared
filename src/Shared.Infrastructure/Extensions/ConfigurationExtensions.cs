using Microsoft.Extensions.Configuration;
using Shared.Abstractions.Options;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for working with configuration and IOptions.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Loads and binds IOptions from configuration by their SectionName property.
    /// </summary>
    /// <typeparam name="T">The IOptions type to load. Must have a SectionName property and parameterless constructor.</typeparam>
    /// <param name="configuration">The configuration to load from.</param>
    /// <returns>An instance of T loaded from the configuration section.</returns>
    public static T LoadOptions<T>(this IConfiguration configuration) where T : class, IOptions, new()
    {
        var options = new T();
        configuration.GetSection(T.SectionName).Bind(options);
        return options;
    }
}
