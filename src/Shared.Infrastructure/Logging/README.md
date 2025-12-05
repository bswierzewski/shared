# Serilog Configuration - Structured Logging with TraceId

Universal Serilog configuration for all projects with automatic TraceId enrichment for request correlation.

## Features

✅ **Automatic TraceId** - Every log includes request TraceId for correlation
✅ **Universal** - Works with Web apps (ASP.NET Core) and Console apps
✅ **Structured Logging** - Logs with context (Application, Environment, MachineName)
✅ **Console + File** - Logs to console and rotating files
✅ **Production Ready** - Security-aware, no sensitive data leakage

## Usage

### Web Applications (ASP.NET Core)

```csharp
using Shared.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with TraceId support
builder.AddSerilog("MyApp.Backend");

// ... other services ...

var app = builder.Build();

// Exception handling first
app.UseExceptionHandler(options => { });

// Request logging with TraceId (MUST be after UseExceptionHandler)
app.UseSerilogRequestLogging();

// ... other middleware ...

await app.RunAsync();
```

### Console Applications

```csharp
using Shared.Infrastructure.Logging;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

using IHost host = Host.CreateDefaultBuilder()
    .AddSerilog(configuration, "MyApp.Console")
    .ConfigureServices(services =>
    {
        // ... your services ...
    })
    .Build();

await host.RunAsync();
```

## Log Output Examples

### Console Output Format

```
[12:30:45 INF] [00-8a7f9c2d4e1b3f5a-9c8e7f6d5a4b-00] HTTP GET /api/contractors/123 responded 200 in 45.2345 ms
[12:30:45 DBG] [00-8a7f9c2d4e1b3f5a-9c8e7f6d5a4b-00] Executing query GetContractorQuery
[12:30:45 ERR] [00-8a7f9c2d4e1b3f5a-9c8e7f6d5a4b-00] An unhandled exception occurred
System.NullReferenceException: Object reference not set to an instance of an object.
   at GetContractorQueryHandler.Handle()
```

### What gets logged automatically

For Web applications:
- `TraceId` - Unique identifier for the entire request
- `RequestPath` - The endpoint called (e.g., `/api/contractors/123`)
- `RequestMethod` - HTTP method (GET, POST, etc.)
- `RequestHost` - The host header
- `UserAgent` - Client user agent
- `UserId` - User identity if authenticated
- `Application` - Your application name
- `Environment` - Development/Production/etc.
- `MachineName` - Server name

## Configuration (appsettings.json)

You can override defaults via configuration:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  },
  "Logging": {
    "FilePath": "Logs/myapp-.txt"
  }
}
```

## File Logging

Logs are automatically written to files:
- **Path**: `Logs/log-.txt` (configurable via `Logging:FilePath`)
- **Rolling**: Daily rotation (log-20231201.txt, log-20231202.txt, etc.)
- **Level**: Only Warning and above (to reduce noise)
- **Format**: Full timestamp with timezone

Example file log:
```
2023-12-01 12:30:45.123 +01:00 [ERR] [00-8a7f9c2d4e1b3f5a-9c8e7f6d5a4b-00] An unhandled exception occurred while processing request GET /api/contractors/123
System.NullReferenceException: Object reference not set to an instance of an object.
   at Autodor.Modules.Contractors.Application.Queries.GetContractor.GetContractorQueryHandler.Handle(...)
```

## Best Practices

### Using TraceId for Debugging

1. **User reports error** - Frontend shows TraceId to user
2. **Search logs** - Grep for TraceId to see entire request flow
3. **Correlate events** - All logs with same TraceId belong to same request

Example:
```bash
# Find all logs for specific request
grep "8a7f9c2d4e1b3f5a" Logs/log-20231201.txt

# Output shows complete request flow:
# [12:30:45] [8a7f...] Request started
# [12:30:45] [8a7f...] User authenticated
# [12:30:45] [8a7f...] Executing query
# [12:30:45] [8a7f...] Database error
# [12:30:45] [8a7f...] Request failed with 500
```

### Structured Logging in Your Code

Use structured logging with properties:

```csharp
// ✅ Good - Structured
_logger.LogInformation(
    "Processing contractor {ContractorId} for user {UserId}",
    contractorId,
    userId);

// ❌ Bad - String interpolation
_logger.LogInformation($"Processing contractor {contractorId} for user {userId}");
```

Properties are automatically indexed and searchable in log aggregation tools (ELK, Seq, Application Insights).

## Troubleshooting

### TraceId shows as empty `[]`

- **Cause**: Missing `app.UseSerilogRequestLogging()` in Program.cs
- **Fix**: Add it AFTER `app.UseExceptionHandler()`

### Logs don't show context properties

- **Cause**: Missing `builder.AddSerilog()` before building app
- **Fix**: Call it right after creating `WebApplicationBuilder`

### File logs not created

- **Cause**: No write permissions to Logs folder
- **Fix**: Ensure app has write access or configure different path

## Migration from Old Logging

If you had manual Serilog configuration:

```diff
- Log.Logger = new LoggerConfiguration()
-     .WriteTo.Console()
-     .CreateLogger();
-
- builder.Host.UseSerilog();

+ builder.AddSerilog("MyApp");
```

Benefits of new approach:
- Automatic TraceId enrichment
- Consistent formatting across all projects
- Production-ready defaults
- Less boilerplate code
