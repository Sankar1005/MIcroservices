using OrderService.Api.Models;

namespace OrderService.Api.Services;

/// <summary>
/// Business logic between <c>OrdersController</c>, <c>IOrderRepository</c> (own database)
/// and <c>IProductServiceClient</c> (the other service).
/// </summary>
public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Checks every product with ProductService, copies name and price, then saves the order.</summary>
    Task<CreateOrderResult> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);

    /// <returns>False when the order does not exist (or was already cancelled).</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
