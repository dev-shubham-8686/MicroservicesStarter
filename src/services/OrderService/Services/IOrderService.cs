using OrderService.Models;
using Shared.Contracts.DTOs;
using Shared.Infrastructure.Exceptions;

namespace OrderService.Services;

public interface IOrderService
{
    Task<List<Order>> GetAllAsync();
    Task<List<Order>> GetByUserIdAsync(Guid userId);
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(Guid userId, CreateOrderRequest request);
    Task UpdateStatusAsync(Guid id, string status);
}


