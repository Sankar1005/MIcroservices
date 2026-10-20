using Microservices.Shared.Contracts.Products;

namespace OrderService.Api.Clients;

/// <summary>
/// OrderService's view of ProductService. The rest of OrderService only talks to this interface,
/// so it does not know (or care) that the data comes over HTTP.
/// </summary>
public interface IProductServiceClient
{
    /// <returns>The product, or null when ProductService answers 404 (missing or deleted).</returns>
    /// <exception cref="ProductServiceUnavailableException">ProductService could not be reached or failed.</exception>
    Task<ProductDto?> GetProductAsync(int productId, CancellationToken cancellationToken = default);
}
