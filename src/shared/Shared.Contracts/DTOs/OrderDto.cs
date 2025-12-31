namespace Shared.Contracts.DTOs;

/// <summary>
/// Order data transfer object
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Order item data transfer object
/// </summary>
public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Request to create an order
/// </summary>
public class CreateOrderRequest
{
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Request item for creating an order
/// </summary>
public class CreateOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Request to update order status
/// </summary>
public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Request for paginated order list
/// </summary>
public class GetOrdersRequest : Common.PagedRequest
{
    public string? Status { get; set; }
}

