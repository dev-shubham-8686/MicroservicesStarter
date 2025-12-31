using ProductService.Models;

namespace ProductService.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(Guid id, Product product);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateStockAsync(Guid id, int quantity);
}

