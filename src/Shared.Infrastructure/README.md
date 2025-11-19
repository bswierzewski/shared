# Shared.Infrastructure

Infrastructure layer library providing implementations for external concerns like data access, caching, messaging, and third-party integrations for Clean Architecture applications.

## Features

- **Authorization Behavior**: Automatic authorization checking via MediatR pipeline
- **Logging Behavior**: Request/response logging with timing information
- **Performance Behavior**: Performance monitoring and warning for slow operations
- **Exception Handling**: Custom exception types (ForbiddenAccessException, ValidationException)
- **Module Management**: Module loading, registration, and lifecycle management
- **Persistence Interceptors**: Automatic audit trail tracking (CreatedAt, UpdatedAt, CreatedBy)
- **EF Core Integration**: Database context configuration and migration support

## Installation

```bash
dotnet add package Shared.Infrastructure
```

## Depends On

- Shared.Abstractions

## Usage

### Registering Infrastructure Services

```csharp
using Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register all infrastructure services
services.AddInfrastructure(configuration);
services.AddModules(typeof(Program).Assembly);

var serviceProvider = services.BuildServiceProvider();
```

### Module Management

```csharp
public class OrdersModule : IModule
{
    public string Name => "Orders";
    public string Version => "1.0.0";

    public void Register(IServiceCollection services)
    {
        // Register module-specific services
        services.AddScoped<IOrderService, OrderService>();
    }

    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Initialize module resources
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
```

### Authorization Pipeline

```csharp
// Automatically validated via AuthorizationBehavior
[Authorize(Roles = "Admin")]
public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}
```

### Audit Trail

```csharp
public class User : AggregateRoot, IAuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

// Automatically updated via AuditableEntityInterceptor
```

### Performance Monitoring

```csharp
// Requests taking longer than threshold trigger warning
public class ExpensiveQuery : IRequest<List<Report>>
{
    // Returns large data set
}
```

## Key Classes

- **AuthorizationBehavior**: MediatR pipeline behavior for authorization
- **LoggingBehavior**: Request/response logging
- **PerformanceBehavior**: Performance monitoring
- **ModuleLoader**: Dynamic module loading from assemblies
- **ModuleRegistry**: Centralized module registration
- **AuditableEntityInterceptor**: Automatic audit field updates

## Dependencies

- Shared.Abstractions
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- Microsoft.EntityFrameworkCore
- MediatR
- FluentValidation

## License

Apache License 2.0
