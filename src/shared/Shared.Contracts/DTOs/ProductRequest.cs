using Shared.Contracts.Validation;

namespace Shared.Contracts.DTOs;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Product name is required")]
    public string Name { get; set; } = string.Empty;

    [Nullable]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    public int Stock { get; set; }
}

public class UpdateProductRequest
{
    [Nullable]
    public string? Name { get; set; }

    [Nullable]
    public string? Description { get; set; }

    [Nullable]
    public decimal? Price { get; set; }

    [Nullable]
    public int? Stock { get; set; }
}

