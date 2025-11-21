using Shared.Abstractions.Modules;

namespace Shared.Users.Infrastructure;

/// <summary>
/// Marker class for the Users module Infrastructure assembly.
/// Used by MediatR to discover and register command/query handlers from this assembly.
/// </summary>
public class InfrastructureAssemblyMarker : IAssemblyMarker
{
}
