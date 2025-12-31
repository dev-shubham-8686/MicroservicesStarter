using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Mappers;
using OrderService.Services;
using Shared.Contracts.Common;
using Shared.Contracts.DTOs;
using Shared.Contracts.Validators;
using Shared.Infrastructure.Controllers;
using Shared.Infrastructure.Exceptions;
using Shared.Infrastructure.Extensions;
using System.Security.Claims;
using FluentValidation;

namespace OrderService.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class OrderController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _createValidator;
    private readonly IValidator<UpdateOrderStatusRequest> _updateStatusValidator;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        IValidator<CreateOrderRequest> createValidator,
        IValidator<UpdateOrderStatusRequest> updateStatusValidator,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _createValidator = createValidator;
        _updateStatusValidator = updateStatusValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders (Admin only)
    /// </summary>
    /// <returns>List of all orders</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Success(orders.Select(o => o.ToDto()).ToList());
    }

    /// <summary>
    /// Get current user's orders
    /// </summary>
    /// <returns>List of user's orders</returns>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyOrders()
    {
        var userId = GetUserId();
        var orders = await _orderService.GetByUserIdAsync(userId);
        return Success(orders.Select(o => o.ToDto()).ToList());
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        
        // Check if user owns the order or is admin
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        
        if (!isAdmin && order.UserId != userId)
        {
            throw new ForbiddenException("You do not have access to this order");
        }

        return Success(order.ToDto());
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var userId = GetUserId();
        var order = await _orderService.CreateAsync(userId, request);
        
        _logger.LogInformation("Order created: {OrderId} by user: {UserId}", order.Id, userId);
        
        return CreatedSuccess(nameof(GetById), new { id = order.Id }, order.ToDto());
    }

    /// <summary>
    /// Update order status (Admin only)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateStatusValidator.ValidateAndThrowAsync(request, cancellationToken);

        await _orderService.UpdateStatusAsync(id, request.Status);
        
        _logger.LogInformation("Order status updated: {OrderId}, Status: {Status}", id, request.Status);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid user identity");
        }
        return userId;
    }
}
