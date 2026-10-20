using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using OrderService.Api.Clients;

namespace OrderService.UnitTests;

/// <summary>
/// Tests the real HTTP client against a fake HTTP handler, so we can see exactly which URL it calls
/// and how it reacts to each kind of answer from ProductService.
/// </summary>
public class ProductServiceClientTests
{
    private sealed class FakeProductServiceHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<string> RequestedUrls { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestedUrls.Add($"{request.Method} {request.RequestUri}");
            return Task.FromResult(respond(request));
        }
    }

    private static (ProductServiceClient Client, FakeProductServiceHandler Handler) CreateClient(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new FakeProductServiceHandler(respond);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5101/") };
        return (new ProductServiceClient(httpClient, NullLogger<ProductServiceClient>.Instance), handler);
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task Calls_GET_api_products_id_on_ProductService()
    {
        var (client, handler) = CreateClient(_ => Json("""{"id":1,"name":"Laptop","price":65000.00,"stockQuantity":25}"""));

        await client.GetProductAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(["GET http://localhost:5101/api/products/1"], handler.RequestedUrls);
    }

    [Fact]
    public async Task Turns_the_json_answer_into_a_ProductDto()
    {
        var (client, _) = CreateClient(_ => Json("""{"id":1,"name":"Laptop","price":65000.00,"stockQuantity":25}"""));

        var product = await client.GetProductAsync(1, TestContext.Current.CancellationToken);

        Assert.NotNull(product);
        Assert.Equal("Laptop", product.Name);
        Assert.Equal(65000.00m, product.Price);
        Assert.Equal(25, product.StockQuantity);
    }

    [Fact]
    public async Task Returns_null_when_ProductService_answers_404()
    {
        var (client, _) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        var product = await client.GetProductAsync(999, TestContext.Current.CancellationToken);

        Assert.Null(product);
    }

    [Fact]
    public async Task Throws_unavailable_when_ProductService_answers_500()
    {
        var (client, _) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<ProductServiceUnavailableException>(
            () => client.GetProductAsync(1, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Throws_unavailable_when_ProductService_is_not_running()
    {
        var (client, _) = CreateClient(_ => throw new HttpRequestException("No connection could be made"));

        await Assert.ThrowsAsync<ProductServiceUnavailableException>(
            () => client.GetProductAsync(1, TestContext.Current.CancellationToken));
    }
}
