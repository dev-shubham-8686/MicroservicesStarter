namespace Shared.Contracts.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

