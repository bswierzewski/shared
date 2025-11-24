using Microsoft.AspNetCore.Routing;

namespace Shared.Abstractions.Modules;

/// <summary>
/// Marker interface for automatic endpoint discovery and registration.
/// Classes implementing this interface will be automatically discovered from assemblies
/// marked with IModuleAssembly and registered in the DI container.
///
/// The MapEndpoints method will be invoked automatically during application startup
/// to configure module HTTP endpoints.
///
/// Usage:
/// <code>
/// public class UserEndpoints : IModuleEndpoints
/// {
///     public void MapEndpoints(IEndpointRouteBuilder endpoints)
///     {
///         var group = endpoints.MapGroup("/api/users")
///             .WithTags("Users");
///
///         group.MapGet("/{id}", GetUserById);
///         group.MapPost("/", CreateUser);
///     }
/// }
/// </code>
/// </summary>
public interface IModuleEndpoints
{
    /// <summary>
    /// Maps HTTP endpoints to the application route builder.
    /// This method is called automatically during application startup.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to configure.</param>
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
