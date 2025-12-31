using OrderService.Models;
using Shared.Contracts.DTOs;

namespace OrderService.Mappers;

public static class OrderMapper
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(ToDto).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    public static OrderItemDto ToDto(this OrderItem orderItem)
    {
        return new OrderItemDto
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            Price = orderItem.Price
        };
    }

    public static Order ToEntity(this CreateOrderRequest request, Guid userId)
    {
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
        return order;
    }

    public static List<OrderDto> ToDtoList(this IEnumerable<Order> orders)
    {
        return orders.Select(ToDto).ToList();
    }
}

