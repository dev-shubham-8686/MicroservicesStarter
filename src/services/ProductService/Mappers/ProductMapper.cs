using ProductService.Models;
using Shared.Contracts.DTOs;

namespace ProductService.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public static Product ToEntity(this CreateProductRequest request)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateFromDto(this Product product, UpdateProductRequest request)
    {
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.UpdatedAt = DateTime.UtcNow;
    }

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> products)
    {
        return products.Select(ToDto).ToList();
    }
}

