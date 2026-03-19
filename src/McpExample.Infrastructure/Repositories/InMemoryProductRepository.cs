using System.Collections.Concurrent;
using McpExample.Domain.Entities;
using McpExample.Domain.Interfaces;

namespace McpExample.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _store = new();

    public InMemoryProductRepository()
    {
        SeedData();
    }

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = _store.Values.OrderBy(p => p.Name).AsEnumerable();
        return Task.FromResult(products);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var products = _store.Values
            .Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name)
            .AsEnumerable();
        return Task.FromResult(products);
    }

    public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _store[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _store[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = _store.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    private void SeedData()
    {
        var products = new[]
        {
            new Product("Laptop Pro 15", "High-performance 15-inch laptop with Intel Core i9", 1499.99m, 50, "Electronics"),
            new Product("Wireless Mouse", "Ergonomic wireless mouse with long battery life", 29.99m, 200, "Electronics"),
            new Product("Standing Desk", "Height-adjustable standing desk for home office", 549.00m, 25, "Furniture"),
            new Product("Coffee Maker", "12-cup programmable coffee maker with thermal carafe", 89.95m, 75, "Appliances"),
            new Product("Noise-Cancelling Headphones", "Over-ear headphones with 30-hour battery", 299.00m, 100, "Electronics"),
        };

        foreach (var product in products)
        {
            _store[product.Id] = product;
        }
    }
}
