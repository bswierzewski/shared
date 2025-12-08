using Shared.Abstractions.Modules;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Marker class for the Infrastructure assembly.
/// Used for MediatR handler registration and assembly scanning.
/// </summary>
public class InfrastructureAssembly : IModuleAssembly
{
}
