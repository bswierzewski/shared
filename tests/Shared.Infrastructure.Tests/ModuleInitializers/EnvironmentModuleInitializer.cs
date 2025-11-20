using System.Runtime.CompilerServices;
using DotNetEnv;

namespace Shared.Infrastructure.Tests.ModuleInitializers;

/// <summary>
/// Automatically loads environment variables from .env file before any test code executes.
/// Uses ModuleInitializer to guarantee execution order - runs before any fixtures or tests.
/// </summary>
internal static class EnvironmentModuleInitializer
{
    /// <summary>
    /// Loads .env file if it exists. Called automatically by the runtime before module initialization.
    /// This ensures environment variables are available for test fixtures and tests.
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        Console.WriteLine($"[{nameof(EnvironmentModuleInitializer)}] Current Directory: {Directory.GetCurrentDirectory()}");

        // Find and load .env file from current directory or parent directories
        var envPath = Env.TraversePath().Load();

        Console.WriteLine($"[{nameof(EnvironmentModuleInitializer)}] .env found and loaded (count: {envPath.Count()})");
    }
}
