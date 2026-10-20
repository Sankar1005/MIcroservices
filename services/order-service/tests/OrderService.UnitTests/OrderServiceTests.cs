using OrderService.Api.Models;
using OrderService.Api.Services;

namespace OrderService.UnitTests;

public class OrderServiceTests
{
    private readonly FakeOrderRepository _repository = new();
    private readonly FakeProductServiceClient _products = new FakeProductServiceClient()
        .WithProduct(1, "Laptop", 65000m, stockQuantity: 25)
        .WithProduct(2, "Wireless Mouse", 899m, stockQuantity: 200);
    private readonly OrderService.Api.Services.OrderService _service;

    public OrderServiceTests()
    {
        _service = new OrderService.Api.Services.OrderService(_repository, _products);
    }

    private static CreateOrderDto Order(params (int ProductId, int Quantity)[] items) => new()
    {
        CustomerName = "Test Customer",
        Items = items.Select(i => new CreateOrderItemDto { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
    };

    [Fact]
    public async Task CreateAsync_asks_ProductService_for_each_product()
    {
        await _service.CreateAsync(Order((1, 1), (2, 3)), TestContext.Current.CancellationToken);

        Assert.Equal([1, 2], _products.RequestedIds);
    }

    [Fact]
    public async Task CreateAsync_copies_name_and_price_from_ProductService_and_calculates_the_total()
    {
        var result = await _service.CreateAsync(Order((1, 1), (2, 3)), TestContext.Current.CancellationToken);

        Assert.Equal(CreateOrderStatus.Created, result.Status);
        var order = Assert.IsType<OrderDto>(result.Order);
        Assert.Equal(65000m + 3 * 899m, order.TotalAmount);
        Assert.Collection(order.Items,
            laptop =>
            {
                Assert.Equal("Laptop", laptop.ProductName);
                Assert.Equal(65000m, laptop.UnitPrice);
                Assert.Equal(65000m, laptop.LineTotal);
            },
            mouse =>
            {
                Assert.Equal("Wireless Mouse", mouse.ProductName);
                Assert.Equal(899m, mouse.UnitPrice);
                Assert.Equal(3, mouse.Quantity);
                Assert.Equal(2697m, mouse.LineTotal);
            });
    }

    [Fact]
    public async Task CreateAsync_merges_the_same_product_into_one_line()
    {
        var result = await _service.CreateAsync(Order((2, 1), (2, 4)), TestContext.Current.CancellationToken);

        var line = Assert.Single(result.Order!.Items);
        Assert.Equal(5, line.Quantity);
        Assert.Equal([2], _products.RequestedIds);
    }

    [Fact]
    public async Task CreateAsync_fails_when_ProductService_does_not_know_the_product()
    {
        var result = await _service.CreateAsync(Order((1, 1), (999, 1)), TestContext.Current.CancellationToken);

        Assert.Equal(CreateOrderStatus.ProductNotFound, result.Status);
        Assert.Contains("999", result.Error);
        Assert.Empty(_repository.AllRows);
    }

    [Fact]
    public async Task CreateAsync_fails_when_there_is_not_enough_stock()
    {
        var result = await _service.CreateAsync(Order((1, 26)), TestContext.Current.CancellationToken);

        Assert.Equal(CreateOrderStatus.NotEnoughStock, result.Status);
        Assert.Contains("Only 25", result.Error);
        Assert.Empty(_repository.AllRows);
    }

    [Fact]
    public async Task CreateAsync_fails_without_saving_when_ProductService_is_down()
    {
        _products.IsDown = true;

        var result = await _service.CreateAsync(Order((1, 1)), TestContext.Current.CancellationToken);

        Assert.Equal(CreateOrderStatus.ProductServiceUnavailable, result.Status);
        Assert.Empty(_repository.AllRows);
    }

    [Fact]
    public async Task Reading_an_order_does_not_call_ProductService()
    {
        var created = await _service.CreateAsync(Order((1, 1)), TestContext.Current.CancellationToken);
        _products.RequestedIds.Clear();
        _products.IsDown = true;

        var order = await _service.GetByIdAsync(created.Order!.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(order);
        Assert.Equal("Laptop", order.Items[0].ProductName);
        Assert.Empty(_products.RequestedIds);
    }

    [Fact]
    public async Task DeleteAsync_is_a_soft_delete()
    {
        var created = await _service.CreateAsync(Order((1, 1)), TestContext.Current.CancellationToken);

        Assert.True(await _service.DeleteAsync(created.Order!.Id, TestContext.Current.CancellationToken));
        Assert.False(await _service.DeleteAsync(created.Order!.Id, TestContext.Current.CancellationToken));
        Assert.Null(await _service.GetByIdAsync(created.Order!.Id, TestContext.Current.CancellationToken));
        Assert.False(Assert.Single(_repository.AllRows).IsActive);
    }
}
