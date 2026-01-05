using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Shared.Contracts.Events;
using System.Security.Claims;
using MassTransit;
namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShippingController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ShippingController> _logger;

    public ShippingController(
        OrderDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<ShippingController> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Create shipment for an order
    /// Complex business logic: Validates order, creates shipment, generates tracking number, publishes events
    /// </summary>
    [HttpPost("order/{orderId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateShipment(Guid orderId, [FromBody] CreateShipmentRequest request)
    {
        try
        {
            // Validate order exists
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            // Business logic: Only create shipment for paid orders
            if (order.Status != "Paid" && order.Status != "Processing")
            {
                return BadRequest(new { message = $"Cannot create shipment for order with status: {order.Status}" });
            }

            // Generate tracking number
            var trackingNumber = GenerateTrackingNumber(request.Carrier);

            var shipment = new Shipment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                TrackingNumber = trackingNumber,
                Carrier = request.Carrier,
                ShippingMethod = request.ShippingMethod,
                Status = "PENDING",
                ShippingAddress = request.ShippingAddress,
                EstimatedDeliveryDate = CalculateEstimatedDeliveryDate(request.ShippingMethod),
                CreatedAt = DateTime.UtcNow
            };

            _context.Shipments.Add(shipment);

            // Update order status
            order.Status = "Processing";
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Create initial tracking history
            var trackingHistory = new ShipmentTrackingHistory
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipment.Id,
                Status = "PENDING",
                Description = "Shipment created and awaiting processing",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ShipmentTrackingHistory.Add(trackingHistory);
            await _context.SaveChangesAsync();

            // Publish shipment created event
            var shipmentEvent = new ShipmentCreatedEvent
            {
                ShipmentId = shipment.Id,
                OrderId = orderId,
                TrackingNumber = trackingNumber,
                Carrier = request.Carrier ?? string.Empty,
                ShippingMethod = request.ShippingMethod ?? string.Empty,
                Status = "PENDING",
                EstimatedDeliveryDate = shipment.EstimatedDeliveryDate,
                CreatedAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(shipmentEvent);
            _logger.LogInformation("Shipment created event published for OrderId: {OrderId}", orderId);

            return CreatedAtAction(nameof(GetShipment), new { shipmentId = shipment.Id }, shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment");
            return StatusCode(500, new { message = "An error occurred creating shipment" });
        }
    }

    /// <summary>
    /// Update shipment status
    /// Complex business logic: Updates status, creates tracking history, publishes events, handles delivery
    /// </summary>
    [HttpPut("{shipmentId}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateShipmentStatus(Guid shipmentId, [FromBody] UpdateShipmentStatusRequest request)
    {
        try
        {
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.Id == shipmentId);

            if (shipment == null)
            {
                return NotFound(new { message = "Shipment not found" });
            }

            var previousStatus = shipment.Status;
            shipment.Status = request.Status;
            shipment.UpdatedAt = DateTime.UtcNow;

            // Business logic: Handle status-specific updates
            if (request.Status == "SHIPPED")
            {
                shipment.ShippedAt = DateTime.UtcNow;
            }
            else if (request.Status == "DELIVERED")
            {
                shipment.ActualDeliveryDate = DateTime.UtcNow;

                // Update order status
                if (shipment.OrderId.HasValue)
                {
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == shipment.OrderId.Value);
                    if (order != null)
                    {
                        order.Status = "Delivered";
                        order.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // Create tracking history entry
            var trackingHistory = new ShipmentTrackingHistory
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipment.Id,
                Status = request.Status,
                Location = request.Location,
                Description = request.Description ?? $"Status updated to {request.Status}",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ShipmentTrackingHistory.Add(trackingHistory);
            await _context.SaveChangesAsync();

            // Publish status updated event
            var statusEvent = new ShipmentStatusUpdatedEvent
            {
                ShipmentId = shipment.Id,
                OrderId = shipment.OrderId ?? Guid.Empty,
                PreviousStatus = previousStatus ?? string.Empty,
                NewStatus = request.Status,
                Location = request.Location,
                Description = request.Description,
                UpdatedAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(statusEvent);

            // If delivered, publish delivery event
            if (request.Status == "DELIVERED")
            {
                var deliveredEvent = new ShipmentDeliveredEvent
                {
                    ShipmentId = shipment.Id,
                    OrderId = shipment.OrderId ?? Guid.Empty,
                    DeliveredAt = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(deliveredEvent);
            }

            _logger.LogInformation("Shipment status updated: {ShipmentId} from {PreviousStatus} to {NewStatus}",
                shipmentId, previousStatus, request.Status);

            return Ok(shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipment status");
            return StatusCode(500, new { message = "An error occurred updating shipment status" });
        }
    }

    /// <summary>
    /// Get shipment details
    /// </summary>
    [HttpGet("{shipmentId}")]
    public async Task<IActionResult> GetShipment(Guid shipmentId)
    {
        try
        {
            var shipment = await _context.Shipments
                .Include(s => s.TrackingHistory)
                .FirstOrDefaultAsync(s => s.Id == shipmentId);

            if (shipment == null)
            {
                return NotFound(new { message = "Shipment not found" });
            }

            return Ok(shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipment");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get shipment by tracking number
    /// </summary>
    [HttpGet("track/{trackingNumber}")]
    public async Task<IActionResult> GetShipmentByTrackingNumber(string trackingNumber)
    {
        try
        {
            var shipment = await _context.Shipments
                .Include(s => s.TrackingHistory)
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

            if (shipment == null)
            {
                return NotFound(new { message = "Shipment not found" });
            }

            return Ok(shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipment by tracking number");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get shipments for an order
    /// </summary>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrderShipments(Guid orderId)
    {
        try
        {
            var shipments = await _context.Shipments
                .Include(s => s.TrackingHistory)
                .Where(s => s.OrderId == orderId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(shipments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order shipments");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    private string GenerateTrackingNumber(string? carrier)
    {
        var prefix = carrier?.ToUpper() switch
        {
            "UPS" => "1Z",
            "FEDEX" => "FDX",
            "DHL" => "DHL",
            "USPS" => "USPS",
            _ => "TRK"
        };

        return $"{prefix}{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    private DateTime? CalculateEstimatedDeliveryDate(string? shippingMethod)
    {
        return shippingMethod?.ToUpper() switch
        {
            "SAME_DAY" => DateTime.UtcNow.AddDays(1),
            "OVERNIGHT" => DateTime.UtcNow.AddDays(1),
            "EXPRESS" => DateTime.UtcNow.AddDays(2),
            "STANDARD" => DateTime.UtcNow.AddDays(5),
            _ => DateTime.UtcNow.AddDays(7)
        };
    }
}

public class CreateShipmentRequest
{
    public string? Carrier { get; set; }
    public string? ShippingMethod { get; set; }
    public string? ShippingAddress { get; set; }
}

public class UpdateShipmentStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
}
