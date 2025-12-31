using OrderService.Models;

namespace OrderService.Services;

public interface IOrderService
{
    Task<List<Order>> GetAllAsync();
    Task<List<Order>> GetByUserIdAsync(Guid userId);
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(Guid userId, List<CreateOrderItemDto> items);
    Task<bool> UpdateStatusAsync(Guid id, string status);
}

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}


