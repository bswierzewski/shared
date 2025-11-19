namespace Shared.Abstractions.Modules;

/// <summary>
/// Registry that maintains information about all loaded modules in the application.
/// </summary>
public interface IModuleRegistry
{
    /// <summary>
    /// Gets all registered modules.
    /// </summary>
    IEnumerable<ModuleInfo> Modules { get; }

    /// <summary>
    /// Gets module information by name.
    /// </summary>
    /// <param name="name">The module name (case-insensitive).</param>
    /// <returns>The module info if found, null otherwise.</returns>
    ModuleInfo? GetModule(string name);

    /// <summary>
    /// Checks if a module with the given name is registered and enabled.
    /// </summary>
    /// <param name="name">The module name (case-insensitive).</param>
    /// <returns>True if the module is registered, false otherwise.</returns>
    bool IsModuleEnabled(string name);
}
