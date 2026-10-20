using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Controllers;
using OrderService.Api.Models;

namespace OrderService.UnitTests;

/// <summary>
/// Checks that each endpoint returns the right HTTP status code, including the ones caused by ProductService.
/// </summary>
public class OrdersControllerTests
{
    private readonly FakeOrderRepository _repository = new();
    private readonly FakeProductServiceClient _products = new FakeProductServiceClient().WithProduct(1, "Laptop", 65000m, stockQuantity: 25);
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _controller = new OrdersController(new OrderService.Api.Services.OrderService(_repository, _products))
        {
            // Problem(...) needs an HttpContext.
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static CreateOrderDto Order(int productId, int quantity) => new()
    {
        CustomerName = "Test Customer",
        Items = [new CreateOrderItemDto { ProductId = productId, Quantity = quantity }]
    };

    [Fact]
    public async Task Create_returns_201_pointing_at_GetById()
    {
        var result = await _controller.Create(Order(1, 2), TestContext.Current.CancellationToken);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(OrdersController.GetById), created.ActionName);
        Assert.Equal(130000m, Assert.IsType<OrderDto>(created.Value).TotalAmount);
    }

    [Theory]
    [InlineData(999, 1)]  // ProductService: product not found
    [InlineData(1, 26)]   // ProductService: only 25 in stock
    public async Task Create_returns_400_when_ProductService_rejects_the_product(int productId, int quantity)
    {
        var result = await _controller.Create(Order(productId, quantity), TestContext.Current.CancellationToken);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task Create_returns_503_when_ProductService_is_down()
    {
        _products.IsDown = true;

        var result = await _controller.Create(Order(1, 1), TestContext.Current.CancellationToken);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, problem.StatusCode);
    }

    [Fact]
    public async Task GetById_returns_404_when_missing()
    {
        var result = await _controller.GetById(999, TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_returns_204_then_404()
    {
        var created = (CreatedAtActionResult)(await _controller.Create(Order(1, 1), TestContext.Current.CancellationToken)).Result!;
        var id = ((OrderDto)created.Value!).Id;

        Assert.IsType<NoContentResult>(await _controller.Delete(id, TestContext.Current.CancellationToken));
        Assert.IsType<NotFoundResult>(await _controller.Delete(id, TestContext.Current.CancellationToken));
    }
}
