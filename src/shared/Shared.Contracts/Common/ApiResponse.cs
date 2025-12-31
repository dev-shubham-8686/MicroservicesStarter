namespace Shared.Contracts.Common;

/// <summary>
/// Standard API response wrapper for all API responses
/// </summary>
/// <typeparam name="T">Type of the data payload</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response data (null if Success is false)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error information (null if Success is true)
    /// </summary>
    public ErrorResponse? Error { get; set; }

    /// <summary>
    /// Response metadata (pagination, timestamps, etc.)
    /// </summary>
    public ResponseMetadata? Metadata { get; set; }

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, ResponseMetadata? metadata = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string errorCode, string message, string? details = null, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = errorCode,
                Message = message,
                Details = details,
                ValidationErrors = validationErrors,
                Timestamp = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Creates a paginated response
    /// </summary>
    public static ApiResponse<T> PaginatedResponse(T data, int pageNumber, int pageSize, int totalCount, int totalPages)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Metadata = new ResponseMetadata
            {
                Pagination = new PaginationMetadata
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                }
            }
        };
    }
}

/// <summary>
/// Standard API response for operations that don't return data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful response with no data
    /// </summary>
    public static new ApiResponse SuccessResponse(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Data = message != null ? new { Message = message } : null
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static new ApiResponse ErrorResponse(string errorCode, string message, string? details = null, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = errorCode,
                Message = message,
                Details = details,
                ValidationErrors = validationErrors,
                Timestamp = DateTime.UtcNow
            }
        };
    }
}

