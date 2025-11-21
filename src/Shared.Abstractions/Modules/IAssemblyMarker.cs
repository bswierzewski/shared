namespace Shared.Abstractions.Modules;

/// <summary>
/// Marker interface for module assemblies.
/// Each module should have classes implementing this interface (ApplicationAssemblyMarker, InfrastructureAssemblyMarker)
/// to mark assemblies for MediatR handler registration.
/// </summary>
public interface IAssemblyMarker
{
}
