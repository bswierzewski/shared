using System.Runtime.CompilerServices;
using DotNetEnv;

namespace Shared.Infrastructure.Tests.ModuleInitializers;

/// <summary>
/// Automatically loads environment variables from .env file before any test code executes.
/// Uses ModuleInitializer to guarantee execution order - runs before any fixtures or tests.
/// </summary>
/// <remarks>
/// CA2255 suppressed: This is intentionally used in a test infrastructure library.
/// It ensures environment variables are loaded before any consuming test projects run.
/// </remarks>
internal static class EnvironmentModuleInitializer
{
    /// <summary>
    /// Loads .env file if it exists. Called automatically by the runtime before module initialization.
    /// This ensures environment variables are available for test fixtures and tests.
    /// </summary>
    [ModuleInitializer]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2255:The ModuleInitializer attribute should not be used in libraries")]
    public static void Initialize()
    {
        Console.WriteLine($"[{nameof(EnvironmentModuleInitializer)}] Current Directory: {Directory.GetCurrentDirectory()}");

        // Find and load .env file from current directory or parent directories
        var envPath = Env.TraversePath().Load();

        Console.WriteLine($"[{nameof(EnvironmentModuleInitializer)}] .env found and loaded (count: {envPath.Count()})");
    }
}
