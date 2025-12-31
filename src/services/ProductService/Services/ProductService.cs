using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using Shared.Contracts.Events;
using Shared.Infrastructure.Exceptions;

namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly ProductDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        ProductDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<ProductService> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new NotFoundException("Product", id);
        }
        return product;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Publish event
        await _publishEndpoint.Publish(new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt
        });

        _logger.LogInformation("Product created: {ProductId}", product.Id);
        return product;
    }

    public async Task<Product> UpdateAsync(Guid id, Product product)
    {
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            throw new NotFoundException("Product", id);
        }

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Product updated: {ProductId}", id);
        return existingProduct;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new NotFoundException("Product", id);
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Product deleted: {ProductId}", id);
    }

    public async Task UpdateStockAsync(Guid id, int quantity)
    {
        var product = await GetByIdAsync(id); // This will throw NotFoundException if not found

        product.Stock += quantity;
        if (product.Stock < 0)
        {
            throw new BusinessRuleException(Shared.Contracts.Common.ErrorCodes.InsufficientStock,
                "Stock cannot be negative");
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Product stock updated: {ProductId}, New Stock: {Stock}", id, product.Stock);
    }
}


