using System.Net;
using System.Net.Http.Json;
using Microservices.Shared.Contracts.Products;
using Polly;

namespace OrderService.Api.Clients;

/// <summary>
/// Calls ProductService over HTTP: GET {BaseUrl}api/products/{id}.
/// The HttpClient it receives is created by IHttpClientFactory (see Program.cs), already configured with
/// the base address and the resilience handler (retries, timeouts, circuit breaker).
/// </summary>
public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductServiceClient> _logger;

    public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Asking ProductService for product {ProductId}", productId);

        try
        {
            using var response = await _httpClient.GetAsync($"api/products/{productId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("ProductService says product {ProductId} does not exist", productId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ProductServiceUnavailableException(
                    $"ProductService returned {(int)response.StatusCode} for product {productId}.");
            }

            // The JSON is turned back into the same ProductDto class ProductService used to create it
            // (shared/src/Microservices.Shared.Contracts).
            return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken)
                ?? throw new ProductServiceUnavailableException($"ProductService returned an empty body for product {productId}.");
        }
        catch (HttpRequestException ex)
        {
            // Connection refused, DNS failure, ... (ProductService is not running).
            throw new ProductServiceUnavailableException("ProductService could not be reached.", ex);
        }
        catch (ExecutionRejectedException ex)
        {
            // Polly gave up: a timeout, or the circuit breaker is open after many failures.
            throw new ProductServiceUnavailableException("ProductService did not respond in time.", ex);
        }
    }
}
