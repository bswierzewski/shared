using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Exceptions;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
};

var builder = WebApplication.CreateBuilder(options);

// Exception handling
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

// OpenAPI/Swagger
builder.Services.AddOpenApi();

// Register modules from auto-generated registry
builder.Services.RegisterModules(builder.Configuration);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Exception handling
app.UseExceptionHandler(options => { });

app.UseAuthentication();
app.UseAuthorization();

// Use all modules (automatic middleware & endpoint configuration)
app.UseModules(builder.Configuration);

// Map OpenAPI endpoint - available at /openapi/v1.json
app.MapOpenApi();

// Initialize all modules (migrations, seeding, etc.)
await app.Services.InitModules();

await app.RunAsync();

/// <summary>
/// Test program class for Shared.Exceptions module integration testing.
/// [GenerateModuleRegistry] triggers source generator to create ModuleRegistry class.
/// </summary>
[GenerateModuleRegistry]
public partial class Program { }
