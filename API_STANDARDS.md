# API Standards and Best Practices

This document outlines the industry-standard API structure, error handling, and best practices implemented in this microservices solution.

## Table of Contents

1. [API Response Structure](#api-response-structure)
2. [Error Handling](#error-handling)
3. [Request Validation](#request-validation)
4. [API Versioning](#api-versioning)
5. [Project Structure](#project-structure)
6. [Best Practices](#best-practices)

## API Response Structure

All API responses follow a standardized format:

### Success Response

```json
{
  "success": true,
  "data": {
    // Response data
  },
  "metadata": {
    "timestamp": "2024-01-01T00:00:00Z",
    "correlationId": "guid-here",
    "version": "1.0",
    "pagination": {
      "pageNumber": 1,
      "pageSize": 10,
      "totalCount": 100,
      "totalPages": 10,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  }
}
```

### Error Response

```json
{
  "success": false,
  "error": {
    "errorCode": "NOT_FOUND",
    "message": "Resource not found",
    "details": "Additional details (only in development)",
    "validationErrors": {
      "fieldName": ["Error message 1", "Error message 2"]
    },
    "timestamp": "2024-01-01T00:00:00Z",
    "correlationId": "guid-here"
  }
}
```

## Error Handling

### Error Codes

Standard error codes are defined in `Shared.Contracts.Common.ErrorCodes`:

- **General Errors (1000-1999)**: INTERNAL_SERVER_ERROR, BAD_REQUEST, NOT_FOUND, UNAUTHORIZED, FORBIDDEN, CONFLICT, VALIDATION_ERROR
- **Authentication Errors (2000-2099)**: INVALID_CREDENTIALS, TOKEN_EXPIRED, USER_NOT_FOUND, USER_ALREADY_EXISTS
- **Product Errors (3000-3099)**: PRODUCT_NOT_FOUND, INSUFFICIENT_STOCK
- **Order Errors (4000-4099)**: ORDER_NOT_FOUND, ORDER_CREATION_FAILED, EMPTY_ORDER_ITEMS
- **Database Errors (5000-5099)**: DATABASE_ERROR, DATABASE_CONNECTION_FAILED

### Exception Types

Custom exception types are available in `Shared.Infrastructure.Exceptions`:

- `ValidationException`: For validation errors (400)
- `NotFoundException`: For resource not found (404)
- `ConflictException`: For conflicts like duplicate entries (409)
- `UnauthorizedException`: For authentication failures (401)
- `ForbiddenException`: For authorization failures (403)
- `BusinessRuleException`: For business rule violations (422)

### Global Exception Handler

The `GlobalExceptionHandlerMiddleware` automatically:
- Catches all exceptions
- Maps exceptions to appropriate HTTP status codes
- Returns standardized error responses
- Includes correlation IDs for tracing
- Logs errors with appropriate levels
- Hides sensitive details in production

## Request Validation

### FluentValidation

All request DTOs are validated using FluentValidation:

```csharp
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}
```

Validators are automatically registered and executed via the `ValidateAndThrowAsync` extension method.

## API Versioning

All APIs are versioned using URL-based versioning:

- Format: `/api/v{version}/[controller]`
- Default version: v1.0
- Example: `/api/v1/product`

API versioning is configured using `Asp.Versioning.Mvc` package.

## Project Structure

### Shared Projects

#### Shared.Contracts
- `Common/`: API response structures, error codes, pagination
- `DTOs/`: Data Transfer Objects for requests and responses
- `Validators/`: FluentValidation validators
- `Events/`: Domain events

#### Shared.Infrastructure
- `Exceptions/`: Custom exception types
- `Middleware/`: Global exception handler, correlation ID middleware
- `Controllers/`: Base API controller
- `Extensions/`: Extension methods

### Service Structure

Each service follows this structure:

```
ServiceName/
├── Controllers/          # API controllers
├── Services/             # Business logic
├── Data/                 # DbContext
├── Models/               # Domain models
├── Mappers/              # Entity to DTO mappers
└── Program.cs            # Startup configuration
```

## Best Practices

### 1. Controller Best Practices

- Inherit from `BaseApiController` for standardized responses
- Use action attributes: `[HttpPost]`, `[HttpGet]`, etc.
- Include XML comments for Swagger documentation
- Use `[ProducesResponseType]` attributes for API documentation
- Validate requests before processing
- Use proper HTTP status codes

Example:

```csharp
[HttpPost]
[ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
    [FromBody] CreateProductRequest request,
    CancellationToken cancellationToken = default)
{
    await _validator.ValidateAndThrowAsync(request, cancellationToken);
    var product = await _service.CreateAsync(request);
    return CreatedSuccess(nameof(GetById), new { id = product.Id }, product.ToDto());
}
```

### 2. Service Best Practices

- Throw custom exceptions instead of returning null
- Use async/await for all I/O operations
- Log important operations
- Keep services focused on business logic
- Use dependency injection

Example:

```csharp
public async Task<Product> GetByIdAsync(Guid id)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null)
    {
        throw new NotFoundException("Product", id);
    }
    return product;
}
```

### 3. Error Handling Best Practices

- Use specific exception types for different error scenarios
- Include meaningful error messages
- Use error codes for client-side error handling
- Log errors with appropriate levels
- Never expose sensitive information in error messages (production)
- Use correlation IDs for distributed tracing

### 4. Validation Best Practices

- Validate all user inputs
- Use FluentValidation for complex validation rules
- Return clear validation error messages
- Group validation errors by field
- Validate business rules in services

### 5. Logging Best Practices

- Use structured logging
- Include correlation IDs in logs
- Log at appropriate levels:
  - `LogInformation`: Successful operations
  - `LogWarning`: Expected errors (validation, not found)
  - `LogError`: Unexpected errors
- Include context in log messages
- Don't log sensitive information

### 6. API Design Best Practices

- Use RESTful conventions
- Use appropriate HTTP methods (GET, POST, PUT, DELETE)
- Use proper HTTP status codes
- Support pagination for list endpoints
- Use consistent naming conventions
- Version your APIs
- Document your APIs (Swagger/OpenAPI)

### 7. Code Organization Best Practices

- Separate concerns (Controllers, Services, Data Access)
- Use DTOs for API contracts
- Use mappers to convert between entities and DTOs
- Keep controllers thin
- Keep services testable
- Use dependency injection
- Follow SOLID principles

### 8. Security Best Practices

- Authenticate all endpoints (except public ones)
- Use role-based authorization where needed
- Validate and sanitize all inputs
- Use HTTPS in production
- Implement rate limiting
- Don't expose sensitive data in responses
- Use secure password hashing (BCrypt)

## Migration Guide

When adding new endpoints:

1. Create DTOs in `Shared.Contracts/DTOs`
2. Create validators in `Shared.Contracts/Validators`
3. Update service interfaces and implementations
4. Create controller actions with proper attributes
5. Add XML comments for documentation
6. Use `BaseApiController` helper methods for responses
7. Throw appropriate custom exceptions
8. Test error scenarios

## Examples

See the existing controllers for complete examples:
- `IdentityService/Controllers/AuthController.cs`
- `ProductService/Controllers/ProductController.cs`
- `OrderService/Controllers/OrderController.cs`

