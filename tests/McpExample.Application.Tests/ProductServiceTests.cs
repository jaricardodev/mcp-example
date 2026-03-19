using FluentAssertions;
using McpExample.Application.DTOs;
using McpExample.Application.Services;
using McpExample.Domain.Entities;
using McpExample.Domain.Interfaces;
using Moq;

namespace McpExample.Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _sut = new ProductService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new("Laptop", "A laptop", 999.99m, 10, "Electronics"),
            new("Mouse", "A mouse", 29.99m, 50, "Electronics"),
        };
        _repositoryMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Laptop");
        result.Should().Contain(p => p.Name == "Mouse");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProductDto()
    {
        // Arrange
        var product = new Product("Laptop", "A laptop", 999.99m, 10, "Electronics");
        _repositoryMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Laptop");
        result.Price.Should().Be(999.99m);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetProductByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ShouldReturnFilteredProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new("Laptop", "A laptop", 999.99m, 10, "Electronics"),
        };
        _repositoryMock.Setup(r => r.GetByCategoryAsync("Electronics", default)).ReturnsAsync(products);

        // Act
        var result = await _sut.GetProductsByCategoryAsync("Electronics");

        // Assert
        result.Should().HaveCount(1);
        result.First().Category.Should().Be("Electronics");
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnCreatedProductDto()
    {
        // Arrange
        var dto = new CreateProductDto("New Product", "Description", 49.99m, 100, "Gadgets");
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        // Act
        var result = await _sut.CreateProductAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.Price.Should().Be(49.99m);
        result.StockQuantity.Should().Be(100);
        result.Category.Should().Be("Gadgets");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductExists_ShouldReturnUpdatedProductDto()
    {
        // Arrange
        var existing = new Product("Old Name", "Old description", 10m, 5, "OldCat");
        var dto = new UpdateProductDto("New Name", "New description", 20m, 10, "NewCat");

        _repositoryMock.Setup(r => r.GetByIdAsync(existing.Id, default)).ReturnsAsync(existing);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), default))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        // Act
        var result = await _sut.UpdateProductAsync(existing.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Price.Should().Be(20m);
        result.Category.Should().Be("NewCat");
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);
        var dto = new UpdateProductDto("Name", "Desc", 10m, 1, "Cat");

        // Act
        var result = await _sut.UpdateProductAsync(Guid.NewGuid(), dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductExists_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteAsync(id, default)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteProductAsync(id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), default)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteProductAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
