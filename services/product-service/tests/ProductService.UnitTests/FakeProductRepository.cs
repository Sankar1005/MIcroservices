using ProductService.Api.Models;
using ProductService.Api.Repositories;

namespace ProductService.UnitTests;

/// <summary>
/// In-memory stand-in for the ADO.NET repository, so unit tests run without a database.
/// It follows the same rules: soft delete, and only active products are read or changed.
/// </summary>
internal sealed class FakeProductRepository : IProductRepository
{
    private readonly List<Product> _products = [];
    private int _nextId = 1;

    public IReadOnlyList<Product> AllRows => _products;

    public Product Add(string name, decimal price, int stockQuantity = 10, bool isActive = true)
    {
        var product = new Product
        {
            Id = _nextId++,
            Name = name,
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        _products.Add(product);
        return product;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> active = _products.Where(p => p.IsActive).ToList();
        return Task.FromResult(active);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_products.FirstOrDefault(p => p.Id == id && p.IsActive));
    }

    public Task<int> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Add(product.Name, product.Price, product.StockQuantity).Id);
    }

    public Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existing = _products.FirstOrDefault(p => p.Id == product.Id && p.IsActive);
        if (existing is null)
        {
            return Task.FromResult(false);
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.UpdatedAt = product.UpdatedAt;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (existing is null)
        {
            return Task.FromResult(false);
        }

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(true);
    }
}
