using ProductService.Models;
using Shared.Infrastructure.Exceptions;

namespace ProductService.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Guid id, Product product);
    Task DeleteAsync(Guid id);
    Task UpdateStockAsync(Guid id, int quantity);
}


