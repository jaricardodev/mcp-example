using McpExample.Application.DTOs;
using McpExample.Application.Interfaces;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpExample.API.McpTools;

/// <summary>
/// MCP tools that expose product operations to LLM clients.
/// Each tool corresponds to a domain operation and delegates to the application service.
/// </summary>
[McpServerToolType]
public class ProductMcpTools
{
    private readonly IProductService _productService;

    public ProductMcpTools(IProductService productService)
    {
        _productService = productService;
    }

    [McpServerTool(Name = "list_products"), Description("Returns a list of all available products.")]
    public async Task<IEnumerable<ProductDto>> ListProducts(CancellationToken cancellationToken)
    {
        return await _productService.GetAllProductsAsync(cancellationToken);
    }

    [McpServerTool(Name = "get_product"), Description("Returns a single product by its unique identifier.")]
    public async Task<ProductDto?> GetProduct(
        [Description("The unique identifier (GUID) of the product.")] Guid id,
        CancellationToken cancellationToken)
    {
        return await _productService.GetProductByIdAsync(id, cancellationToken);
    }

    [McpServerTool(Name = "get_products_by_category"), Description("Returns all products in the specified category.")]
    public async Task<IEnumerable<ProductDto>> GetProductsByCategory(
        [Description("The category name to filter products by (e.g., 'Electronics', 'Furniture').")] string category,
        CancellationToken cancellationToken)
    {
        return await _productService.GetProductsByCategoryAsync(category, cancellationToken);
    }

    [McpServerTool(Name = "create_product"), Description("Creates a new product and returns the created product details.")]
    public async Task<ProductDto> CreateProduct(
        [Description("The name of the product.")] string name,
        [Description("A description of the product.")] string description,
        [Description("The price of the product in USD.")] decimal price,
        [Description("The number of units in stock.")] int stockQuantity,
        [Description("The category the product belongs to.")] string category,
        CancellationToken cancellationToken)
    {
        var dto = new CreateProductDto(name, description, price, stockQuantity, category);
        return await _productService.CreateProductAsync(dto, cancellationToken);
    }

    [McpServerTool(Name = "update_product"), Description("Updates an existing product and returns the updated product details. Returns null if not found.")]
    public async Task<ProductDto?> UpdateProduct(
        [Description("The unique identifier (GUID) of the product to update.")] Guid id,
        [Description("The new name of the product.")] string name,
        [Description("The new description of the product.")] string description,
        [Description("The new price of the product in USD.")] decimal price,
        [Description("The new number of units in stock.")] int stockQuantity,
        [Description("The new category the product belongs to.")] string category,
        CancellationToken cancellationToken)
    {
        var dto = new UpdateProductDto(name, description, price, stockQuantity, category);
        return await _productService.UpdateProductAsync(id, dto, cancellationToken);
    }

    [McpServerTool(Name = "delete_product"), Description("Deletes a product by its unique identifier. Returns true if deleted, false if not found.")]
    public async Task<bool> DeleteProduct(
        [Description("The unique identifier (GUID) of the product to delete.")] Guid id,
        CancellationToken cancellationToken)
    {
        return await _productService.DeleteProductAsync(id, cancellationToken);
    }
}
