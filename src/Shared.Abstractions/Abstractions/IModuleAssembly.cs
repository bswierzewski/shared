namespace Shared.Abstractions.Abstractions;

/// <summary>
/// Marker interface for module assemblies.
/// Assemblies implementing this interface will be automatically scanned for:
/// - MediatR handlers (IRequestHandler, INotificationHandler)
/// - FluentValidation validators (IValidator, AbstractValidator)
/// - Module endpoints (IModuleEndpoints)
///
/// Usage: Create marker classes in each module layer (Application, Infrastructure, etc.)
/// Example:
/// <code>
/// public sealed class ApplicationAssembly : IModuleAssembly { }
/// public sealed class InfrastructureAssembly : IModuleAssembly { }
/// </code>
/// </summary>
public interface IModuleAssembly
{
}