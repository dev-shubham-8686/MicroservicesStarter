using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Mappers;
using ProductService.Services;
using Shared.Contracts.Common;
using Shared.Contracts.DTOs;
using Shared.Contracts.Validators;
using Shared.Infrastructure.Controllers;
using Shared.Infrastructure.Exceptions;
using Shared.Infrastructure.Extensions;
using FluentValidation;

namespace ProductService.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class ProductController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductService productService,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        var productDtos = products.Select(p => p.ToDto()).ToList();
        
        return Success(productDtos);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            throw new NotFoundException("Product", id);
        }

        return Success(product.ToDto());
    }

    /// <summary>
    /// Create a new product (Admin only)
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var product = request.ToEntity();
        var createdProduct = await _productService.CreateAsync(product);
        
        _logger.LogInformation("Product created: {ProductId} by user", createdProduct.Id);
        
        return CreatedSuccess(nameof(GetById), new { id = createdProduct.Id }, createdProduct.ToDto());
    }

    /// <summary>
    /// Update a product (Admin only)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Product update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var existingProduct = await _productService.GetByIdAsync(id);
        existingProduct.UpdateFromDto(request);
        var updatedProduct = await _productService.UpdateAsync(id, existingProduct);

        _logger.LogInformation("Product updated: {ProductId}", id);
        return Success(updatedProduct.ToDto());
    }

    /// <summary>
    /// Delete a product (Admin only)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result)
        {
            throw new NotFoundException("Product", id);
        }

        _logger.LogInformation("Product deleted: {ProductId}", id);
        return NoContent();
    }
}
