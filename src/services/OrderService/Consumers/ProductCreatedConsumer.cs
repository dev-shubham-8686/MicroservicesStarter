using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace OrderService.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        _logger.LogInformation("Product created event received: ProductId={ProductId}, Name={Name}, Price={Price}",
            context.Message.ProductId, context.Message.Name, context.Message.Price);
        
        // Here you could update local cache, send notifications, etc.
        return Task.CompletedTask;
    }
}

