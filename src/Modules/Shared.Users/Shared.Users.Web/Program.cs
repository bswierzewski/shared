using Shared.Abstractions.Modules;
using Shared.Users.Infrastructure.Extensions.Supabase;

var builder = WebApplication.CreateBuilder(args);

// Register modules from auto-generated registry
builder.Services.RegisterModules(builder.Configuration);

builder.Services.AddAuthentication()
    .AddSupabaseJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Use all modules (automatic middleware & endpoint configuration)
app.UseModules(builder.Configuration);

// Initialize all modules (migrations, seeding, etc.)
await app.Services.InitModules();

await app.RunAsync();

/// <summary>
/// Main program class used for integration testing.
/// [GenerateModuleRegistry] triggers source generator to create ModuleRegistry class.
/// </summary>
[GenerateModuleRegistry]
public partial class Program { }
