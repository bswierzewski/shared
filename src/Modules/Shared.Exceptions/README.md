# Shared.Exceptions Module

Exception testing module for validating exception handling and ProblemDetails responses in ASP.NET Core applications.

## Purpose

This module provides test endpoints that intentionally throw various types of exceptions to verify that:
- `ApiExceptionHandler` correctly transforms exceptions into RFC 7807 ProblemDetails
- All exception types return proper HTTP status codes
- ProblemDetails contain required fields (traceId, errors, etc.)
- Frontend clients receive consistent error responses

## Structure

```
Shared.Exceptions/
├── Shared.Exceptions.Application/
│   ├── Commands/
│   │   ├── ThrowValidationException/     # 400 Bad Request
│   │   ├── ThrowNotFoundException/        # 404 Not Found
│   │   ├── ThrowUnauthorizedException/    # 401 Unauthorized
│   │   ├── ThrowForbiddenException/       # 403 Forbidden
│   │   └── ThrowServerException/          # 500 Internal Server Error
│   └── ApplicationAssembly.cs
└── Shared.Exceptions.Infrastructure/
    ├── Endpoints/
    │   └── ExceptionEndpoints.cs
    ├── ExceptionModule.cs
    └── InfrastructureAssembly.cs
```

## Endpoints

All endpoints require authorization and are available at `/api/exceptions/`:

| Endpoint | Exception Type | Status | Purpose |
|----------|---------------|--------|---------|
| `GET /api/exceptions/validation` | ValidationException | 400 | Test validation error responses with errors field |
| `GET /api/exceptions/not-found` | NotFoundException | 404 | Test resource not found responses |
| `GET /api/exceptions/unauthorized` | UnauthorizedAccessException | 401 | Test authentication required responses |
| `GET /api/exceptions/forbidden` | ForbiddenAccessException | 403 | Test permission denied responses |
| `GET /api/exceptions/server-error` | InvalidOperationException | 500 | Test unexpected error responses |

## Configuration

The module can be disabled via appsettings:

```json
{
  "Modules": {
    "Exceptions": {
      "Enabled": false
    }
  }
}
```

## Usage in Projects

### 1. Add Project References

In your host project (e.g., `Snippet.Web.csproj`):

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\..\shared\src\Modules\Shared.Exceptions\Shared.Exceptions.Infrastructure\Shared.Exceptions.Infrastructure.csproj" />
</ItemGroup>
```

### 2. Module Auto-Discovery

The module is automatically discovered and registered by the module system. No manual registration required!

### 3. Test Endpoints

```bash
# Test validation errors
curl -H "Authorization: Bearer {token}" http://localhost:7000/api/exceptions/validation

# Expected Response (400 Bad Request):
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation error",
  "status": 400,
  "instance": "/api/exceptions/validation",
  "traceId": "00-1234567890abcdef-1234567890abcdef-00",
  "errors": {
    "Title": ["The Title field is required."],
    "Content": ["The Content field must be at least 10 characters long."],
    "Tags": ["At least one tag is required."]
  }
}
```

## End-to-End Tests

Example test to verify exception handling:

```csharp
[Fact]
public async Task ValidationException_ShouldReturn400WithProblemDetails()
{
    // Act
    var response = await _client.GetAsync("/api/exceptions/validation");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problemDetails.Should().NotBeNull();
    problemDetails!.Status.Should().Be(400);
    problemDetails.Extensions.Should().ContainKey("traceId");
    problemDetails.Extensions.Should().ContainKey("errors");
}
```

## Design Decisions

### Why Commands Instead of Direct Exceptions?

1. **Clean Architecture** - Follows CQRS pattern used throughout the application
2. **Testability** - Commands can be unit tested independently
3. **Consistency** - Same pattern as all other endpoints in the system
4. **Flexibility** - Easy to add logging, metrics, or additional behavior

### Why a Separate Module?

1. **Reusability** - Can be used across multiple projects (Snippet, Users, etc.)
2. **Optional** - Can be disabled in production via appsettings
3. **Isolation** - Test code doesn't pollute business logic
4. **Discoverability** - Auto-registered with other modules

## Security Considerations

- All endpoints require authorization (`RequireAuthorization()`)
- Should be disabled in production environments
- No sensitive data is exposed in exception messages
- TraceId allows correlation with server logs for debugging

## See Also

- `Shared.Infrastructure.Exceptions.ApiExceptionHandler` - Exception handler implementation
- `Shared.Infrastructure.OpenApi.OpenApiExtensions` - ProblemDetails schema configuration
- RFC 7807 - Problem Details for HTTP APIs
