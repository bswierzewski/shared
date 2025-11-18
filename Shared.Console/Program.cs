using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Modules;

// Load .env file
DotEnv.Load();

// Build configuration from environment variables
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

Console.WriteLine("=== Shared Console Application ===");
Console.WriteLine($"App Name: {configuration["APP:NAME"] ?? "N/A"}");
Console.WriteLine($"App Version: {configuration["APP:VERSION"] ?? "N/A"}");
Console.WriteLine();

// Debug: Check module enabled setting
Console.WriteLine("=== Configuration Check ===");
Console.WriteLine($"USERS:MODULE:ENABLED = {configuration["USERS:MODULE:ENABLED"] ?? "not set"}");
Console.WriteLine($"users:module:enabled = {configuration["users:module:enabled"] ?? "not set"}");
Console.WriteLine();

// Build service collection
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);

// Add modules infrastructure (loads, registers, and configures all modules)
Console.WriteLine("Loading and registering modules...");

services.AddModules(configuration);

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Display module information
var moduleRegistry = serviceProvider.GetRequiredService<IModuleRegistry>();
Console.WriteLine("=== Module Registry ===");
foreach (var moduleInfo in moduleRegistry.Modules)
{
    Console.WriteLine($"\nModule: {moduleInfo.Name}");

    Console.WriteLine("  Permissions:");
    foreach (var permission in moduleInfo.Permissions)
    {
        Console.WriteLine($"    - {permission.Name}: {permission.DisplayName}");
        if (!string.IsNullOrEmpty(permission.Description))
        {
            Console.WriteLine($"      {permission.Description}");
        }
    }

    Console.WriteLine("  Roles:");
    foreach (var role in moduleInfo.Roles)
    {
        Console.WriteLine($"    - {role.Name}: {role.DisplayName}");
        if (!string.IsNullOrEmpty(role.Description))
        {
            Console.WriteLine($"      {role.Description}");
        }
        Console.WriteLine($"      Permissions: {string.Join(", ", role.Permissions.Select(p => p.Name))}");
    }
}

Console.WriteLine();
Console.WriteLine("=== Infrastructure Ready ===");
Console.WriteLine("- MediatR with pipeline behaviors (Logging, Authorization, Validation, Performance)");
Console.WriteLine("- FluentValidation validators registered");
Console.WriteLine("- Module services registered");
Console.WriteLine();

// Example: Check if specific services are registered
Console.WriteLine("=== Registered Services Check ===");
var userService = serviceProvider.GetService<Shared.Abstractions.Security.IUser>();
Console.WriteLine($"IUser service: {(userService != null ? "Registered" : "Not found")}");

var mediator = serviceProvider.GetService<MediatR.IMediator>();
Console.WriteLine($"IMediator service: {(mediator != null ? "Registered" : "Not found")}");

Console.WriteLine();
Console.WriteLine("Application initialized successfully!");
