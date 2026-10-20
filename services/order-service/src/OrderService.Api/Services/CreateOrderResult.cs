using OrderService.Api.Models;

namespace OrderService.Api.Services;

public enum CreateOrderStatus
{
    Created,
    ProductNotFound,
    NotEnoughStock,
    ProductServiceUnavailable
}

/// <summary>
/// The outcome of placing an order. The controller turns each status into an HTTP status code.
/// </summary>
public record CreateOrderResult(CreateOrderStatus Status, OrderDto? Order = null, string? Error = null)
{
    public static CreateOrderResult Created(OrderDto order) => new(CreateOrderStatus.Created, order);

    public static CreateOrderResult Failed(CreateOrderStatus status, string error) => new(status, Error: error);
}
