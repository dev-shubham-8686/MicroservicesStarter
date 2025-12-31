using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Common;

namespace Shared.Infrastructure.Controllers;

/// <summary>
/// Base controller with helper methods for standardized API responses
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Creates a successful response with standardized format
    /// </summary>
    protected ActionResult<ApiResponse<T>> Success<T>(T data, ResponseMetadata? metadata = null)
    {
        var response = ApiResponse<T>.SuccessResponse(data, metadata);
        return base.Ok(response);
    }

    /// <summary>
    /// Creates a created response (201) with standardized format
    /// </summary>
    protected ActionResult<ApiResponse<T>> CreatedSuccess<T>(string actionName, object routeValues, T data)
    {
        var response = ApiResponse<T>.SuccessResponse(data);
        return base.CreatedAtAction(actionName, routeValues, response);
    }

    /// <summary>
    /// Creates a paginated response
    /// </summary>
    protected ActionResult<ApiResponse<T>> SuccessPaginated<T>(
        T data,
        int pageNumber,
        int pageSize,
        int totalCount,
        int totalPages)
    {
        var response = ApiResponse<T>.PaginatedResponse(data, pageNumber, pageSize, totalCount, totalPages);
        return base.Ok(response);
    }
}

