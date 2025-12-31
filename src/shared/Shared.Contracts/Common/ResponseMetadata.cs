namespace Shared.Contracts.Common;

/// <summary>
/// Metadata associated with API responses
/// </summary>
public class ResponseMetadata
{
    /// <summary>
    /// Pagination information
    /// </summary>
    public PaginationMetadata? Pagination { get; set; }

    /// <summary>
    /// Response timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// API version
    /// </summary>
    public string? Version { get; set; }
}

/// <summary>
/// Pagination metadata
/// </summary>
public class PaginationMetadata
{
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }
}

