using Shared.Abstractions.Modules;

namespace Shared.Infrastructure.Modules;

/// <summary>
/// Default implementation of IModuleRegistry that stores module information in memory.
/// </summary>
public sealed class ModuleRegistry : IModuleRegistry
{
    private readonly Dictionary<string, ModuleInfo> _modules = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IEnumerable<ModuleInfo> Modules => _modules.Values;

    /// <inheritdoc />
    public ModuleInfo? GetModule(string name)
    {
        return _modules.TryGetValue(name, out var module) ? module : null;
    }

    /// <inheritdoc />
    public bool IsModuleEnabled(string name)
    {
        return _modules.ContainsKey(name);
    }

    /// <summary>
    /// Registers a module in the registry.
    /// </summary>
    /// <param name="moduleInfo">The module information to register.</param>
    internal void Register(ModuleInfo moduleInfo)
    {
        _modules[moduleInfo.Name] = moduleInfo;
    }
}
