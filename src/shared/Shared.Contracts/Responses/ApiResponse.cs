namespace Shared.Contracts.Responses;

/// <summary>
/// Standard API Response wrapper following Google/Microsoft style patterns
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// The actual data payload
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error information if request failed
    /// </summary>
    public ErrorDetail? Error { get; set; }

    /// <summary>
    /// Additional metadata about the response
    /// </summary>
    public ResponseMetadata? Metadata { get; set; }

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = true,
            Metadata = message != null ? new ResponseMetadata { Message = message } : null
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, string? code = null, object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ErrorDetail
            {
                Message = message,
                Code = code ?? "ERROR",
                Details = details
            }
        };
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, string[]> validationErrors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ErrorDetail
            {
                Message = "Validation failed",
                Code = "VALIDATION_ERROR",
                Details = validationErrors
            }
        };
    }
}

/// <summary>
/// Error detail information
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Machine-readable error code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (e.g., validation errors)
    /// </summary>
    public object? Details { get; set; }
}

/// <summary>
/// Response metadata
/// </summary>
public class ResponseMetadata
{
    /// <summary>
    /// Optional message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request ID for tracking
    /// </summary>
    public string? RequestId { get; set; }
}

/// <summary>
/// Empty response for operations that don't return data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResponse(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Metadata = message != null ? new ResponseMetadata { Message = message } : null
        };
    }

    public static new ApiResponse ErrorResponse(string message, string? code = null, object? details = null)
    {
        return new ApiResponse
        {
            Success = false,
            Error = new ErrorDetail
            {
                Message = message,
                Code = code ?? "ERROR",
                Details = details
            }
        };
    }

    public static new ApiResponse ValidationErrorResponse(Dictionary<string, string[]> validationErrors)
    {
        return new ApiResponse
        {
            Success = false,
            Error = new ErrorDetail
            {
                Message = "Validation failed",
                Code = "VALIDATION_ERROR",
                Details = validationErrors
            }
        };
    }
}

