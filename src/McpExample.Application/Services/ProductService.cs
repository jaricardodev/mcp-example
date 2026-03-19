using McpExample.Application.DTOs;
using McpExample.Application.Interfaces;
using McpExample.Domain.Entities;
using McpExample.Domain.Interfaces;

namespace McpExample.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetByCategoryAsync(category, cancellationToken);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = new Product(dto.Name, dto.Description, dto.Price, dto.StockQuantity, dto.Category);
        var created = await _repository.AddAsync(product, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return null;

        existing.Update(dto.Name, dto.Description, dto.Price, dto.StockQuantity, dto.Category);
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static ProductDto MapToDto(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.Category,
            product.CreatedAt,
            product.UpdatedAt
        );
}
