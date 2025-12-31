namespace Shared.Contracts.Common;

/// <summary>
/// Base class for paginated requests
/// </summary>
public class PagedRequest
{
    /// <summary>
    /// Page number (1-based, default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default: 10, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets the validated page number
    /// </summary>
    public int GetValidatedPageNumber()
    {
        return PageNumber < 1 ? 1 : PageNumber;
    }

    /// <summary>
    /// Gets the validated page size
    /// </summary>
    public int GetValidatedPageSize(int maxPageSize = 100)
    {
        if (PageSize < 1) return 10;
        return PageSize > maxPageSize ? maxPageSize : PageSize;
    }
}

