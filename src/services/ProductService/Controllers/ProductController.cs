using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Services;
using Shared.Contracts.DTOs;
using Shared.Contracts.Responses;
using Shared.Contracts.Validation;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Product>>>> GetAll()
    {
        try
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse<List<Product>>.SuccessResponse(products));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return StatusCode(500, ApiResponse<List<Product>>.ErrorResponse("An error occurred while retrieving products", "INTERNAL_ERROR"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Product>>> GetById(Guid id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<Product>.ErrorResponse("Product not found", "NOT_FOUND"));
            }

            return Ok(ApiResponse<Product>.SuccessResponse(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {Id}", id);
            return StatusCode(500, ApiResponse<Product>.ErrorResponse("An error occurred while retrieving product", "INTERNAL_ERROR"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<Product>>> Create([FromBody] CreateProductRequest request)
    {
        // Validate request at action level
        var validationErrors = RequestValidator.Validate(request);
        if (validationErrors != null)
        {
            _logger.LogWarning("Validation failed for create product request");
            return BadRequest(ApiResponse<Product>.ValidationErrorResponse(validationErrors));
        }

        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CreatedAt = DateTime.UtcNow
            };

            var createdProduct = await _productService.CreateAsync(product);
            return CreatedAtAction(
                nameof(GetById),
                new { id = createdProduct.Id },
                ApiResponse<Product>.SuccessResponse(createdProduct, "Product created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, ApiResponse<Product>.ErrorResponse("An error occurred while creating product", "INTERNAL_ERROR"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<Product>>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        // Validate request at action level
        var validationErrors = RequestValidator.Validate(request);
        if (validationErrors != null)
        {
            _logger.LogWarning("Validation failed for update product request");
            return BadRequest(ApiResponse<Product>.ValidationErrorResponse(validationErrors));
        }

        try
        {
            var existingProduct = await _productService.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound(ApiResponse<Product>.ErrorResponse("Product not found", "NOT_FOUND"));
            }

            // Update only provided fields
            if (request.Name != null) existingProduct.Name = request.Name;
            if (request.Description != null) existingProduct.Description = request.Description;
            if (request.Price.HasValue) existingProduct.Price = request.Price.Value;
            if (request.Stock.HasValue) existingProduct.Stock = request.Stock.Value;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            var updatedProduct = await _productService.UpdateAsync(id, existingProduct);
            if (updatedProduct == null)
            {
                return NotFound(ApiResponse<Product>.ErrorResponse("Product not found", "NOT_FOUND"));
            }

            return Ok(ApiResponse<Product>.SuccessResponse(updatedProduct, "Product updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {Id}", id);
            return StatusCode(500, ApiResponse<Product>.ErrorResponse("An error occurred while updating product", "INTERNAL_ERROR"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var result = await _productService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found", "NOT_FOUND"));
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting product", "INTERNAL_ERROR"));
        }
    }
}

