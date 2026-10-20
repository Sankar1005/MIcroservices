using ProductService.Api.Models;

namespace ProductService.UnitTests;

public class ProductServiceTests
{
    private readonly FakeProductRepository _repository = new();
    private readonly ProductService.Api.Services.ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService.Api.Services.ProductService(_repository);
    }

    [Fact]
    public async Task GetAllAsync_returns_only_active_products()
    {
        _repository.Add("Laptop", 65000m);
        _repository.Add("Old mouse", 500m, isActive: false);

        var products = await _service.GetAllAsync(TestContext.Current.CancellationToken);

        var product = Assert.Single(products);
        Assert.Equal("Laptop", product.Name);
    }

    [Fact]
    public async Task GetByIdAsync_maps_every_field_to_the_dto()
    {
        var saved = _repository.Add("Laptop", 65000m, stockQuantity: 25);

        var dto = await _service.GetByIdAsync(saved.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.Equal(saved.Id, dto.Id);
        Assert.Equal("Laptop", dto.Name);
        Assert.Equal(65000m, dto.Price);
        Assert.Equal(25, dto.StockQuantity);
        Assert.Equal(saved.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_the_product_does_not_exist()
    {
        var dto = await _service.GetByIdAsync(999, TestContext.Current.CancellationToken);

        Assert.Null(dto);
    }

    [Fact]
    public async Task CreateAsync_saves_the_product_and_returns_it_with_the_new_id()
    {
        var request = new CreateProductDto { Name = "HD Webcam", Description = "1080p", Price = 2499m, StockQuantity = 40 };

        var dto = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

        var row = Assert.Single(_repository.AllRows);
        Assert.Equal(row.Id, dto.Id);
        Assert.Equal("HD Webcam", dto.Name);
        Assert.Equal(2499m, dto.Price);
        Assert.Equal(40, dto.StockQuantity);
    }

    [Fact]
    public async Task UpdateAsync_changes_the_fields_and_sets_UpdatedAt()
    {
        var saved = _repository.Add("HD Webcam", 2499m);
        var before = DateTime.UtcNow;
        var request = new UpdateProductDto { Name = "HD Webcam Pro", Price = 2999m, StockQuantity = 35 };

        var updated = await _service.UpdateAsync(saved.Id, request, TestContext.Current.CancellationToken);

        Assert.True(updated);
        Assert.Equal("HD Webcam Pro", saved.Name);
        Assert.Equal(2999m, saved.Price);
        Assert.Equal(35, saved.StockQuantity);
        Assert.NotNull(saved.UpdatedAt);
        Assert.True(saved.UpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateAsync_returns_false_when_the_product_does_not_exist()
    {
        var request = new UpdateProductDto { Name = "Nothing", Price = 10m, StockQuantity = 1 };

        var updated = await _service.UpdateAsync(999, request, TestContext.Current.CancellationToken);

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_is_a_soft_delete_so_the_product_is_hidden_but_kept()
    {
        var saved = _repository.Add("HD Webcam", 2499m);

        var deleted = await _service.DeleteAsync(saved.Id, TestContext.Current.CancellationToken);

        Assert.True(deleted);
        Assert.Null(await _service.GetByIdAsync(saved.Id, TestContext.Current.CancellationToken));
        var row = Assert.Single(_repository.AllRows);
        Assert.False(row.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_returns_false_the_second_time()
    {
        var saved = _repository.Add("HD Webcam", 2499m);
        await _service.DeleteAsync(saved.Id, TestContext.Current.CancellationToken);

        var deletedAgain = await _service.DeleteAsync(saved.Id, TestContext.Current.CancellationToken);

        Assert.False(deletedAgain);
    }
}
