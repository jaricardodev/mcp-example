using FluentAssertions;
using McpExample.Domain.Entities;

namespace McpExample.Application.Tests;

public class ProductEntityTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateProduct()
    {
        // Act
        var product = new Product("Laptop", "A laptop", 999.99m, 10, "Electronics");

        // Assert
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be("Laptop");
        product.Description.Should().Be("A laptop");
        product.Price.Should().Be(999.99m);
        product.StockQuantity.Should().Be(10);
        product.Category.Should().Be("Electronics");
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException(string name)
    {
        // Act
        var act = () => new Product(name, "desc", 10m, 1, "Cat");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Product("Name", "desc", -1m, 1, "Cat");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("price");
    }

    [Fact]
    public void Constructor_WithNegativeStockQuantity_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Product("Name", "desc", 10m, -1, "Cat");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("stockQuantity");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProduct()
    {
        // Arrange
        var product = new Product("Old Name", "Old desc", 10m, 5, "OldCat");
        var originalCreatedAt = product.CreatedAt;

        // Act
        product.Update("New Name", "New desc", 20m, 10, "NewCat");

        // Assert
        product.Name.Should().Be("New Name");
        product.Description.Should().Be("New desc");
        product.Price.Should().Be(20m);
        product.StockQuantity.Should().Be(10);
        product.Category.Should().Be("NewCat");
        product.CreatedAt.Should().Be(originalCreatedAt);
        product.UpdatedAt.Should().BeOnOrAfter(originalCreatedAt);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var product = new Product("Name", "desc", 10m, 1, "Cat");

        // Act
        var act = () => product.Update("", "desc", 10m, 1, "Cat");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }
}
