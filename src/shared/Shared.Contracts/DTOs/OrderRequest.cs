using Shared.Contracts.Validation;

namespace Shared.Contracts.DTOs;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "Order items are required")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Price is required")]
    public decimal Price { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
}

