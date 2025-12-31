using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Shared.Contracts.Events;
using Shared.Contracts.DTOs;
using Shared.Infrastructure.Exceptions;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        OrderDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<OrderService> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<Order> GetByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            throw new NotFoundException("Order", id);
        }

        return order;
    }

    public async Task<Order> CreateAsync(Guid userId, CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new BusinessRuleException(Shared.Contracts.Common.ErrorCodes.EmptyOrderItems,
                "Order must contain at least one item");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Publish event
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = userId,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new Shared.Contracts.Events.OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        });

        _logger.LogInformation("Order created: {OrderId} for user: {UserId}", order.Id, userId);
        return order;
    }

    public async Task UpdateStatusAsync(Guid id, string status)
    {
        var order = await GetByIdAsync(id); // This will throw NotFoundException if not found

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order status updated: {OrderId}, Status: {Status}", id, status);
    }
}


