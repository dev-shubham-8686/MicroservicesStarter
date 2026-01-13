using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;
using Shared.Contracts.DTOs;
using Shared.Contracts.Responses;
using Shared.Contracts.Validation;
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<Order>>>> GetAll()
    {
        try
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(ApiResponse<List<Order>>.SuccessResponse(orders));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return StatusCode(500, ApiResponse<List<Order>>.ErrorResponse("An error occurred while retrieving orders", "INTERNAL_ERROR"));
        }
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<ApiResponse<List<Order>>>> GetMyOrders()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<List<Order>>.ErrorResponse("User ID not found in token", "UNAUTHORIZED"));
            }

            var orders = await _orderService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<List<Order>>.SuccessResponse(orders));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user orders");
            return StatusCode(500, ApiResponse<List<Order>>.ErrorResponse("An error occurred while retrieving orders", "INTERNAL_ERROR"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Order>>> GetById(Guid id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(ApiResponse<Order>.ErrorResponse("Order not found", "NOT_FOUND"));
            }

            // Check if user owns the order or is admin
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && (string.IsNullOrEmpty(userIdClaim) || order.UserId != Guid.Parse(userIdClaim)))
            {
                return Forbid();
            }

            return Ok(ApiResponse<Order>.SuccessResponse(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by ID: {Id}", id);
            return StatusCode(500, ApiResponse<Order>.ErrorResponse("An error occurred while retrieving order", "INTERNAL_ERROR"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Order>>> Create([FromBody] CreateOrderRequest request)
    {
        // Validate request at action level
        var validationErrors = RequestValidator.ValidateNested(request);
        if (validationErrors != null)
        {
            _logger.LogWarning("Validation failed for create order request");
            return BadRequest(ApiResponse<Order>.ValidationErrorResponse(validationErrors));
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<Order>.ErrorResponse("User ID not found in token", "UNAUTHORIZED"));
            }

            // Convert request DTOs to service DTOs
            var orderItems = request.Items.Select(item => new CreateOrderItemDto
            {
                //Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList() ?? Enumerable.Empty<CreateOrderItemDto>().ToList();

            var order = await _orderService.CreateAsync(userId, orderItems);
            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                ApiResponse<Order>.SuccessResponse(order, "Order created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, ApiResponse<Order>.ErrorResponse("An error occurred while creating order", "INTERNAL_ERROR"));
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        // Validate request at action level
        var validationErrors = RequestValidator.Validate(request);
        if (validationErrors != null)
        {
            _logger.LogWarning("Validation failed for update order status request");
            return BadRequest(ApiResponse.ValidationErrorResponse(validationErrors));
        }

        try
        {
            var result = await _orderService.UpdateStatusAsync(id, request.Status);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("Order not found", "NOT_FOUND"));
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status: {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating order status", "INTERNAL_ERROR"));
        }
    }
}

