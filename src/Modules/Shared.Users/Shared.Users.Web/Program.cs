using Shared.Abstractions.Modules;
using Shared.Infrastructure.Modules;
using Shared.Users.Infrastructure.Extensions.JwtBearers;

var builder = WebApplication.CreateBuilder(args);

// Load and register all modules
var modules = ModuleLoader.LoadModules();

builder.Services.AddSingleton<IReadOnlyCollection<IModule>>(modules.AsReadOnly());

builder.Services.RegisterModules(modules, builder.Configuration);

builder.Services.AddAuthentication()
    .AddTestJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Use all modules (automatic middleware & endpoint configuration)
app.UseModules(modules, builder.Configuration);

// Initialize all modules (migrations, seeding, etc.)
await app.Services.InitializeModules(modules);

await app.RunAsync();

/// <summary>
/// Main program class used for integration testing.
/// </summary>
public partial class Program { }
