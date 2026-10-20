using Microservices.Shared.Contracts.Products;
using OrderService.Api.Clients;
using OrderService.Api.Models;
using OrderService.Api.Repositories;

namespace OrderService.UnitTests;

/// <summary>
/// Stands in for ProductService: no HTTP, just a dictionary of products.
/// Set <see cref="IsDown"/> to simulate ProductService not running.
/// </summary>
internal sealed class FakeProductServiceClient : IProductServiceClient
{
    private readonly Dictionary<int, ProductDto> _products = [];

    public bool IsDown { get; set; }
    public List<int> RequestedIds { get; } = [];

    public FakeProductServiceClient WithProduct(int id, string name, decimal price, int stockQuantity = 100)
    {
        _products[id] = new ProductDto { Id = id, Name = name, Price = price, StockQuantity = stockQuantity };
        return this;
    }

    public Task<ProductDto?> GetProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        RequestedIds.Add(productId);

        if (IsDown)
        {
            throw new ProductServiceUnavailableException("ProductService could not be reached.");
        }

        return Task.FromResult(_products.GetValueOrDefault(productId));
    }
}

/// <summary>
/// In-memory stand-in for the ADO.NET repository, so unit tests run without a database.
/// </summary>
internal sealed class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];
    private int _nextId = 1;

    public IReadOnlyList<Order> AllRows => _orders;

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Order> active = _orders.Where(o => o.IsActive).ToList();
        return Task.FromResult(active);
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id && o.IsActive));
    }

    public Task<int> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        order.Id = _nextId++;
        order.IsActive = true;
        order.CreatedAt = DateTime.UtcNow;
        foreach (var item in order.Items)
        {
            item.OrderId = order.Id;
        }

        _orders.Add(order);
        return Task.FromResult(order.Id);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id && o.IsActive);
        if (order is null)
        {
            return Task.FromResult(false);
        }

        order.IsActive = false;
        order.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(true);
    }
}
