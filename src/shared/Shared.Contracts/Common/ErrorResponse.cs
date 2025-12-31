namespace Shared.Contracts.Common;

/// <summary>
/// Standard error response structure
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Machine-readable error code
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (stack traces only in development)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Validation errors grouped by field name
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Request correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Standard error codes used across all services
/// </summary>
public static class ErrorCodes
{
    // General Errors (1000-1999)
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string BadRequest = "BAD_REQUEST";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string UnprocessableEntity = "UNPROCESSABLE_ENTITY";

    // Authentication Errors (2000-2099)
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string TokenInvalid = "TOKEN_INVALID";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
    public const string AccountLocked = "ACCOUNT_LOCKED";

    // Product Errors (3000-3099)
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string ProductAlreadyExists = "PRODUCT_ALREADY_EXISTS";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";
    public const string InvalidProductData = "INVALID_PRODUCT_DATA";

    // Order Errors (4000-4099)
    public const string OrderNotFound = "ORDER_NOT_FOUND";
    public const string OrderCreationFailed = "ORDER_CREATION_FAILED";
    public const string InvalidOrderStatus = "INVALID_ORDER_STATUS";
    public const string OrderUpdateFailed = "ORDER_UPDATE_FAILED";
    public const string EmptyOrderItems = "EMPTY_ORDER_ITEMS";

    // Database Errors (5000-5099)
    public const string DatabaseError = "DATABASE_ERROR";
    public const string DatabaseConnectionFailed = "DATABASE_CONNECTION_FAILED";

    // External Service Errors (6000-6099)
    public const string ExternalServiceError = "EXTERNAL_SERVICE_ERROR";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string Timeout = "TIMEOUT";
}

