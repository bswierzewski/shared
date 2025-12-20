using DotNetEnv;
using Shared.Abstractions.Modules;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Logging;
using Shared.Users.Infrastructure.Extensions.Supabase;

// Load environment variables from .env file BEFORE creating builder
// clobberExistingVars: false ensures Docker/CI/CD environment variables take precedence
if (File.Exists(".env"))
    Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with TraceId support
builder.AddSerilog("Shared.Host");

// Exception handling
builder.Services.AddProblemDetails();

// OpenAPI/Swagger with enhanced ProblemDetails schemas
builder.Services.AddOpenApi(options => options.AddProblemDetailsSchemas());

// Register modules from auto-generated registry
builder.Services.RegisterModules(builder.Configuration);

builder.Services.AddAuthentication()
    .AddSupabaseJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

// Exception handling
app.UseExceptionHandler();

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
/// Host application for local development and testing.
/// [GenerateModuleRegistry] triggers source generator to create ModuleRegistry class.
/// </summary>
[GenerateModuleRegistry]
public partial class Program { }
