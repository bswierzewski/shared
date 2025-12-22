using Shared.Abstractions.Abstractions;

namespace Shared.Exceptions.Application;

/// <summary>
/// Marker class for the Exceptions module Application assembly.
/// Enables automatic discovery and registration of:
/// - MediatR handlers (commands, queries, notifications)
/// - FluentValidation validators
/// </summary>
public sealed class ApplicationAssembly : IModuleAssembly
{
}
