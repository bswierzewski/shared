using DotNetEnv;
using Shared.Exceptions.Infrastructure;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Logging;
using Shared.Users.Infrastructure;
using Shared.Users.Infrastructure.Extensions.Supabase;

// Load environment variables from .env file BEFORE creating builder
if (File.Exists(".env"))
    Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

builder.Services.AddProblemDetails(options =>
    options.AddCustomConfiguration(builder.Environment));

builder.Services.AddOpenApi(options => options.AddProblemDetailsSchemas());

builder.Services.RegisterModules(builder.Configuration,
    new UsersModule(),
    new ExceptionsModule()
    );

builder.Services.AddAuthentication()
    .AddSupabaseJwtBearer();

builder.Services.AddAuthorization();

var app = builder.Build();

// Exception handling
app.UseExceptionHandler();

// Request logging with TraceId (MUST be after UseExceptionHandler)
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseModules(builder.Configuration);

// Map OpenAPI endpoint - available at /openapi/v1.json
app.MapOpenApi();

// Initialize all modules (migrations, seeding, etc.)
await app.Services.InitModules();
await app.RunAsync();

public partial class Program { }
