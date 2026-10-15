using ProductService.Models;

namespace ProductService.Services;

/// <summary>
/// Business logic between <c>ProductsController</c> and <c>IProductRepository</c>.
/// Works with DTOs on the outside and with the <see cref="Product"/> model on the inside.
/// </summary>
public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    /// <returns>False when the product does not exist (or was deleted).</returns>
    Task<bool> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);

    /// <returns>False when the product does not exist (or was already deleted).</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
