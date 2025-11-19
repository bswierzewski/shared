# Shared.Abstractions

Domain layer library providing essential building blocks for Domain-Driven Design (DDD) and Clean Architecture applications.

## Features

- **Abstract Base Classes**: Entity, AggregateRoot, ValueObject, DomainEvent
- **Authorization**: Role-based access control (RBAC) with attributes and permissions
- **Module System**: Modular architecture support with module registration
- **Options Pattern**: IOptions interface for configuration classes
- **Domain Events**: Support for capturing and handling domain events
- **Extensions**: Useful extension methods for common operations

## Installation

```bash
dotnet add package Shared.Abstractions
```

## Usage

### Creating Entities and Aggregate Roots

```csharp
using Shared.Abstractions;

public class User : AggregateRoot
{
    public string Email { get; set; }
    public string FullName { get; set; }
}
```

### Using Domain Events

```csharp
public class User : AggregateRoot, IHasDomainEvent
{
    private List<IDomainEvent> _domainEvents = new();

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void CreateUser(string email, string name)
    {
        Email = email;
        FullName = name;

        _domainEvents.Add(new UserCreatedEvent(this));
    }
}
```

### Authorization with Attributes

```csharp
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    [Authorize(Permissions = "user.write")]
    [HttpPost]
    public IActionResult Create(CreateUserRequest request)
    {
        // Implementation
    }
}
```

### Configuration Options

```csharp
public class DatabaseOptions : IOptions
{
    public static string SectionName => "Database";

    public string ConnectionString { get; set; }
    public int CommandTimeout { get; set; } = 30;
}

// In Startup.cs
services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
```

## Key Interfaces

- **IAggregateRoot**: Marker interface for aggregate roots
- **IHasDomainEvent**: Support domain event capturing
- **IOptions**: Marker for options/configuration classes
- **IUser**: Represents the current user context
- **IModule**: Module registration interface

## Dependencies

- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.AspNetCore.Http.Abstractions
- Microsoft.EntityFrameworkCore

## License

Apache License 2.0
