using Microservices.Shared.Contracts.Products;
using Microsoft.AspNetCore.Mvc;
using ProductService.Api.Controllers;
using ProductService.Api.Models;

namespace ProductService.UnitTests;

/// <summary>
/// Checks that each endpoint returns the right HTTP status code.
/// (Request-body validation / 400 is done by [ApiController] before the action runs, so it is not tested here.)
/// </summary>
public class ProductsControllerTests
{
    private readonly FakeProductRepository _repository = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(new ProductService.Api.Services.ProductService(_repository));
    }

    [Fact]
    public async Task GetAll_returns_200_with_the_list()
    {
        _repository.Add("Laptop", 65000m);

        var result = await _controller.GetAll(TestContext.Current.CancellationToken);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IReadOnlyList<ProductDto>>(ok.Value);
        Assert.Single(products);
    }

    [Fact]
    public async Task GetById_returns_200_when_found()
    {
        var saved = _repository.Add("Laptop", 65000m);

        var result = await _controller.GetById(saved.Id, TestContext.Current.CancellationToken);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(saved.Id, Assert.IsType<ProductDto>(ok.Value).Id);
    }

    [Fact]
    public async Task GetById_returns_404_when_missing()
    {
        var result = await _controller.GetById(999, TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_returns_201_pointing_at_GetById()
    {
        var request = new CreateProductDto { Name = "HD Webcam", Price = 2499m, StockQuantity = 40 };

        var result = await _controller.Create(request, TestContext.Current.CancellationToken);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<ProductDto>(created.Value);
        Assert.Equal(nameof(ProductsController.GetById), created.ActionName);
        Assert.Equal(dto.Id, created.RouteValues?["id"]);
    }

    [Fact]
    public async Task Update_returns_204_when_found_and_404_when_missing()
    {
        var saved = _repository.Add("HD Webcam", 2499m);
        var request = new UpdateProductDto { Name = "HD Webcam Pro", Price = 2999m, StockQuantity = 35 };

        var found = await _controller.Update(saved.Id, request, TestContext.Current.CancellationToken);
        var missing = await _controller.Update(999, request, TestContext.Current.CancellationToken);

        Assert.IsType<NoContentResult>(found);
        Assert.IsType<NotFoundResult>(missing);
    }

    [Fact]
    public async Task Delete_returns_204_then_404()
    {
        var saved = _repository.Add("HD Webcam", 2499m);

        var first = await _controller.Delete(saved.Id, TestContext.Current.CancellationToken);
        var second = await _controller.Delete(saved.Id, TestContext.Current.CancellationToken);

        Assert.IsType<NoContentResult>(first);
        Assert.IsType<NotFoundResult>(second);
    }
}
