using Microservices.Shared.Contracts.Products;
using OrderService.Api.Clients;
using OrderService.Api.Models;
using OrderService.Api.Repositories;

namespace OrderService.Api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IProductServiceClient _productServiceClient;

    public OrderService(IOrderRepository repository, IProductServiceClient productServiceClient)
    {
        _repository = repository;
        _productServiceClient = productServiceClient;
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _repository.GetAllAsync(cancellationToken);
        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ToDto(order);
    }

    public async Task<CreateOrderResult> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        // The same product twice in one request becomes one line with the quantities added up.
        var requestedLines = dto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        var order = new Order { CustomerName = dto.CustomerName };

        foreach (var line in requestedLines)
        {
            // ---- The call to the other microservice (HTTP GET to ProductService) ----
            ProductDto? product;
            try
            {
                product = await _productServiceClient.GetProductAsync(line.ProductId, cancellationToken);
            }
            catch (ProductServiceUnavailableException ex)
            {
                return CreateOrderResult.Failed(CreateOrderStatus.ProductServiceUnavailable, ex.Message);
            }

            if (product is null)
            {
                return CreateOrderResult.Failed(CreateOrderStatus.ProductNotFound,
                    $"Product {line.ProductId} does not exist.");
            }

            if (product.StockQuantity < line.Quantity)
            {
                return CreateOrderResult.Failed(CreateOrderStatus.NotEnoughStock,
                    $"Only {product.StockQuantity} of product {product.Id} ({product.Name}) in stock; {line.Quantity} requested.");
            }

            // Snapshot: copy name and price now. If ProductService changes them later, this order keeps the old values.
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = line.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        var newId = await _repository.CreateAsync(order, cancellationToken);

        // Read the order back so the response includes the values the database filled in (Id, CreatedAt).
        var created = await _repository.GetByIdAsync(newId, cancellationToken)
            ?? throw new InvalidOperationException($"Order {newId} was inserted but could not be read back.");

        return CreateOrderResult.Created(ToDto(created));
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }

    private static OrderDto ToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };
    }
}
