namespace Shared.Contracts.DTOs;

/// <summary>
/// Product data transfer object
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create a product
/// </summary>
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// Request to update a product
/// </summary>
public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// Request to update product stock
/// </summary>
public class UpdateStockRequest
{
    public int Quantity { get; set; }
}

/// <summary>
/// Request for paginated product list
/// </summary>
public class GetProductsRequest : Common.PagedRequest
{
    public string? SearchTerm { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

