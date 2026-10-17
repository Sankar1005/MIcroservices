namespace ProductService.Api.Models;

/// <summary>
/// One row of the product.Products table (defined in database/Microservices.Database).
/// Internal to ProductService: clients receive <c>ProductDto</c> from Microservices.Shared.Contracts.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
