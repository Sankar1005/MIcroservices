using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Models;
using OrderService.Api.Services;

namespace OrderService.Api.Controllers;

/// <summary>
/// HTTP endpoints for orders. Kept thin: all work is done by <see cref="IOrderService"/>.
/// [ApiController] automatically returns 400 when a request body fails validation.
/// </summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET api/orders
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    // GET api/orders/5
    [HttpGet("{id:int}")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    // POST api/orders  (calls ProductService for every product in the order)
    [HttpPost]
    [ProducesResponseType<OrderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(dto, cancellationToken);

        return result.Status switch
        {
            CreateOrderStatus.Created =>
                CreatedAtAction(nameof(GetById), new { id = result.Order!.Id }, result.Order),
            CreateOrderStatus.ProductNotFound or CreateOrderStatus.NotEnoughStock =>
                Problem(title: "The order cannot be placed.", detail: result.Error, statusCode: StatusCodes.Status400BadRequest),
            CreateOrderStatus.ProductServiceUnavailable =>
                Problem(title: "ProductService is unavailable. Please try again later.", detail: result.Error,
                        statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => throw new InvalidOperationException($"Unknown status {result.Status}.")
        };
    }

    // DELETE api/orders/5  (soft delete = cancel)
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _orderService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
