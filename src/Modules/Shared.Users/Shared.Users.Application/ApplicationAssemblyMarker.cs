using Shared.Abstractions.Modules;

namespace Shared.Users.Application;

/// <summary>
/// Marker class for the Users module Application assembly.
/// Used by MediatR to discover and register command/query handlers from this assembly.
/// </summary>
public class ApplicationAssemblyMarker : IAssemblyMarker
{
}
