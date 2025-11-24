using Shared.Abstractions.Modules;

namespace Shared.Users.Application;

/// <summary>
/// Marker class for the Users module Application assembly.
/// Enables automatic discovery and registration of:
/// - MediatR handlers (commands, queries, notifications)
/// - FluentValidation validators
/// </summary>
public sealed class ApplicationAssembly : IModuleAssembly
{
}
