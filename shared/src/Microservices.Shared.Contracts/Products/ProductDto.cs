namespace Microservices.Shared.Contracts.Products;

/// <summary>
/// A product as returned by ProductService (GET api/products and GET api/products/{id}).
/// OrderService reads this type when it asks ProductService for product details,
/// so renaming or removing a property is a breaking change for other services.
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
