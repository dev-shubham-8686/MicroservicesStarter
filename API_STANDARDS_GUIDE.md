# API Standards Guide
## Google/Microsoft Style Request/Response Patterns

This guide explains the industry-standard API request/response patterns implemented in this project, following Google and Microsoft best practices in a simple, easy-to-use way.

---

## Table of Contents

1. [Overview](#overview)
2. [Standard Response Format](#standard-response-format)
3. [Request Validation](#request-validation)
4. [Usage Examples](#usage-examples)
5. [Best Practices](#best-practices)
6. [Error Handling](#error-handling)

---

## Overview

### What We've Implemented

✅ **Standard API Response Wrapper** - Consistent response format across all APIs  
✅ **Custom Validation** - Simple Required/Nullable validation (no FluentValidation)  
✅ **Action-Level Validation** - Validation happens in controller actions  
✅ **Google/Microsoft Style** - Industry-standard patterns made simple  

### Key Principles

1. **Consistency** - All APIs return the same response structure
2. **Simplicity** - Easy to understand and use
3. **Validation** - Only checks Required/Nullable (no complex rules)
4. **Action-Level** - Validation in controllers, not filters/middleware

---

## Standard Response Format

### Success Response

```json
{
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Product Name",
    "price": 99.99
  },
  "success": true,
  "metadata": {
    "message": "Product created successfully",
    "timestamp": "2024-12-15T10:30:00Z",
    "requestId": "req-12345"
  }
}
```

### Error Response

```json
{
  "success": false,
  "error": {
    "message": "Validation failed",
    "code": "VALIDATION_ERROR",
    "details": {
      "email": ["Email is required"],
      "password": ["Password is required"]
    }
  }
}
```

### Response Structure

```csharp
public class ApiResponse<T>
{
    public T? Data { get; set; }           // The actual data
    public bool Success { get; set; }      // Success flag
    public ErrorDetail? Error { get; set; } // Error information
    public ResponseMetadata? Metadata { get; set; } // Additional info
}
```

---

## Request Validation

### Validation Attributes

We use two simple attributes:

#### `[Required]` - Field is required

```csharp
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
```

#### `[Nullable]` - Field can be null

```csharp
public class UpdateProductRequest
{
    [Nullable]
    public string? Name { get; set; }

    [Nullable]
    public string? Description { get; set; }
}
```

### Validation Rules

✅ **Required fields** - Cannot be null, empty, or whitespace  
✅ **Nullable fields** - Can be null (no validation)  
✅ **Nested objects** - Validates nested objects recursively  
✅ **Collections** - Checks if empty for required collections  

### What We DON'T Validate

❌ No format validation (email format, phone format, etc.)  
❌ No range validation (min/max values)  
❌ No regex patterns  
❌ No custom business rules  

**Why?** Keep it simple! Only check if field is required or nullable.

---

## Usage Examples

### Example 1: Register User

**Request:**
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Success Response (200):**
```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe"
    }
  },
  "success": true,
  "metadata": {
    "message": "User registered successfully",
    "timestamp": "2024-12-15T10:30:00Z"
  }
}
```

**Validation Error Response (400):**
```json
{
  "success": false,
  "error": {
    "message": "Validation failed",
    "code": "VALIDATION_ERROR",
    "details": {
      "email": ["Email is required"],
      "password": ["Password is required"]
    }
  }
}
```

### Example 2: Create Product

**Request:**
```http
POST /api/product
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "stock": 50
}
```

**Success Response (201):**
```json
{
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99,
    "stock": 50,
    "createdAt": "2024-12-15T10:30:00Z"
  },
  "success": true,
  "metadata": {
    "message": "Product created successfully",
    "timestamp": "2024-12-15T10:30:00Z"
  }
}
```

### Example 3: Update Product (Partial Update)

**Request:**
```http
PUT /api/product/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Updated Laptop",
  "price": 899.99
}
```

**Success Response (200):**
```json
{
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Updated Laptop",
    "description": "High-performance laptop",
    "price": 899.99,
    "stock": 50
  },
  "success": true,
  "metadata": {
    "message": "Product updated successfully",
    "timestamp": "2024-12-15T10:30:00Z"
  }
}
```

**Note:** Only provided fields are updated (partial update).

### Example 4: Create Order

**Request:**
```http
POST /api/order
Authorization: Bearer {token}
Content-Type: application/json

{
  "items": [
    {
      "productId": "123e4567-e89b-12d3-a456-426614174000",
      "quantity": 2,
      "price": 999.99
    }
  ]
}
```

**Success Response (201):**
```json
{
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "userId": "user-id-here",
    "totalAmount": 1999.98,
    "status": "Pending",
    "items": [
      {
        "id": "item-id",
        "productId": "123e4567-e89b-12d3-a456-426614174000",
        "quantity": 2,
        "price": 999.99
      }
    ],
    "createdAt": "2024-12-15T10:30:00Z"
  },
  "success": true,
  "metadata": {
    "message": "Order created successfully",
    "timestamp": "2024-12-15T10:30:00Z"
  }
}
```

**Validation Error Response (400):**
```json
{
  "success": false,
  "error": {
    "message": "Validation failed",
    "code": "VALIDATION_ERROR",
    "details": {
      "Items": ["Order items are required"],
      "Items[0].ProductId": ["Product ID is required"],
      "Items[0].Quantity": ["Quantity is required"]
    }
  }
}
```

---

## Controller Implementation

### How to Use in Controllers

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<Product>>> Create([FromBody] CreateProductRequest request)
{
    // 1. Validate request at action level
    var validationErrors = RequestValidator.Validate(request);
    if (validationErrors != null)
    {
        return BadRequest(ApiResponse<Product>.ValidationErrorResponse(validationErrors));
    }

    // 2. Process request
    var product = await _productService.CreateAsync(request);

    // 3. Return success response
    return Ok(ApiResponse<Product>.SuccessResponse(product, "Product created successfully"));
}
```

### Validation for Nested Objects

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<Order>>> Create([FromBody] CreateOrderRequest request)
{
    // Use ValidateNested for objects with nested properties
    var validationErrors = RequestValidator.ValidateNested(request);
    if (validationErrors != null)
    {
        return BadRequest(ApiResponse<Order>.ValidationErrorResponse(validationErrors));
    }

    // Process request...
}
```

---

## Best Practices

### 1. Always Use Standard Response Format

✅ **Do:**
```csharp
return Ok(ApiResponse<Product>.SuccessResponse(product));
```

❌ **Don't:**
```csharp
return Ok(product); // Inconsistent format
```

### 2. Validate at Action Level

✅ **Do:**
```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<Product>>> Create([FromBody] CreateProductRequest request)
{
    var validationErrors = RequestValidator.Validate(request);
    if (validationErrors != null)
        return BadRequest(ApiResponse<Product>.ValidationErrorResponse(validationErrors));
    // ...
}
```

❌ **Don't:**
- Use filters or middleware for validation
- Skip validation
- Use FluentValidation

### 3. Use Appropriate HTTP Status Codes

- **200 OK** - Success with data
- **201 Created** - Resource created successfully
- **400 Bad Request** - Validation errors or bad input
- **401 Unauthorized** - Authentication required
- **403 Forbidden** - Not authorized
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error

### 4. Provide Meaningful Error Messages

✅ **Do:**
```csharp
[Required(ErrorMessage = "Email is required")]
public string Email { get; set; }
```

❌ **Don't:**
```csharp
[Required] // No custom message
public string Email { get; set; }
```

### 5. Handle Exceptions Properly

```csharp
try
{
    var result = await _service.DoSomethingAsync();
    return Ok(ApiResponse<Result>.SuccessResponse(result));
}
catch (NotFoundException ex)
{
    return NotFound(ApiResponse<Result>.ErrorResponse(ex.Message, "NOT_FOUND"));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error occurred");
    return StatusCode(500, ApiResponse<Result>.ErrorResponse("An error occurred", "INTERNAL_ERROR"));
}
```

---

## Error Handling

### Error Response Structure

```json
{
  "success": false,
  "error": {
    "message": "Human-readable error message",
    "code": "MACHINE_READABLE_CODE",
    "details": {
      // Additional error details (e.g., validation errors)
    }
  }
}
```

### Common Error Codes

- `VALIDATION_ERROR` - Request validation failed
- `NOT_FOUND` - Resource not found
- `UNAUTHORIZED` - Authentication required
- `FORBIDDEN` - Not authorized
- `INTERNAL_ERROR` - Server error
- `REGISTRATION_FAILED` - Registration failed
- `LOGIN_FAILED` - Login failed

### Error Response Examples

#### Validation Error
```json
{
  "success": false,
  "error": {
    "message": "Validation failed",
    "code": "VALIDATION_ERROR",
    "details": {
      "email": ["Email is required"],
      "password": ["Password is required"]
    }
  }
}
```

#### Not Found Error
```json
{
  "success": false,
  "error": {
    "message": "Product not found",
    "code": "NOT_FOUND"
  }
}
```

#### Unauthorized Error
```json
{
  "success": false,
  "error": {
    "message": "User ID not found in token",
    "code": "UNAUTHORIZED"
  }
}
```

---

## Request DTO Examples

### Simple Request

```csharp
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
```

### Request with Nullable Fields

```csharp
public class UpdateProductRequest
{
    [Nullable]
    public string? Name { get; set; }

    [Nullable]
    public string? Description { get; set; }

    [Nullable]
    public decimal? Price { get; set; }

    [Nullable]
    public int? Stock { get; set; }
}
```

### Request with Nested Objects

```csharp
public class CreateOrderRequest
{
    [Required(ErrorMessage = "Order items are required")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Price is required")]
    public decimal Price { get; set; }
}
```

---

## Summary

### What We've Implemented

✅ Standard API response wrapper (Google/Microsoft style)  
✅ Custom validation (Required/Nullable only)  
✅ Action-level validation  
✅ Consistent error handling  
✅ Simple and easy to use  

### Key Takeaways

1. **Always use `ApiResponse<T>`** for consistent responses
2. **Validate at action level** using `RequestValidator.Validate()`
3. **Use `[Required]` and `[Nullable]`** attributes only
4. **Return appropriate HTTP status codes**
5. **Provide meaningful error messages**

### Next Steps

1. ✅ Use `ApiResponse<T>` in all controllers
2. ✅ Add `[Required]` or `[Nullable]` to request DTOs
3. ✅ Validate requests at action level
4. ✅ Return consistent error responses

---

**Happy API Building!** 🚀

For questions or issues, refer to the code examples in the controllers.

