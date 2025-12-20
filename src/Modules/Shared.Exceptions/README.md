# Shared.Exceptions Module

Exception testing module for validating error handling and exception scenarios.

## Overview

The Shared.Exceptions module provides a set of endpoints for testing different error handling scenarios in the application. It includes commands for:

- **Unhandled Errors**: Tests how the application handles unexpected exceptions
- **Success Responses**: Tests successful operation handling
- **Error Responses**: Tests ErrorOr pattern error handling
- **Role-Based Access Control**: Tests authorization when required roles are missing

## Features

- Exception handling validation
- ErrorOr pattern testing
- Role-based access control testing
- CQRS command pattern implementation
- OpenAPI/Swagger documentation

## Project Structure

### Application Layer (`Shared.Exceptions.Application`)

Contains CQRS commands and handlers for testing different error scenarios:

- `Commands/UnhandledErrorCommand` - Throws an unhandled exception
- `Commands/SuccessResponseCommand` - Returns a successful response
- `Commands/ErrorResponseCommand` - Returns an error response using ErrorOr
- `Commands/RoleProtectedCommand` - Tests role-based authorization

### Infrastructure Layer (`Shared.Exceptions.Infrastructure`)

Provides HTTP endpoints and module integration:

- `Endpoints/ExceptionEndpoints` - Maps HTTP POST endpoints for testing
- `ExceptionsModule` - IModule implementation for registering services and endpoints

## API Endpoints

All endpoints require authentication.

### POST /api/exceptions/unhandled-error
Tests unhandled error handling by throwing an exception.

**Response**: 500 Internal Server Error

### POST /api/exceptions/success
Tests successful response handling.

**Response**: 200 OK
```json
{
  "value": "Success response"
}
```

### POST /api/exceptions/error
Tests error response handling using ErrorOr pattern.

**Response**: 400 Bad Request
```json
{
  "errors": ["This is a test error response"]
}
```

### POST /api/exceptions/role-protected
Tests role-based access control. Requires `admin` role.

**Response**: 200 OK (if user has admin role)
```json
{
  "value": "Role protected response - user has admin role"
}
```

**Response**: 403 Forbidden (if user lacks admin role)

## Usage

The ExceptionsModule should be registered in your application's module system:

```csharp
services.AddModule<ExceptionsModule>();
```

Then configure the module:

```csharp
app.UseModule<ExceptionsModule>();
```

## Dependencies

- MediatR - CQRS pattern implementation
- ErrorOr - Error handling pattern
- Microsoft.AspNetCore.Authorization - Authorization support
- Shared.Infrastructure - Common infrastructure utilities
- Shared.Abstractions - Common abstractions

## License

Apache-2.0
