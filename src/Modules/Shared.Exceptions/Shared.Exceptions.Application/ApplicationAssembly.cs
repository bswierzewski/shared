using Shared.Abstractions.Modules;

namespace Shared.Exceptions.Application;

/// <summary>
/// Marker class for the Application assembly.
/// Used for MediatR handler registration and assembly scanning.
/// </summary>
public class ApplicationAssembly : IModuleAssembly
{
}
