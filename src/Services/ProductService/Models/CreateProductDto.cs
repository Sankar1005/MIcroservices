using System.ComponentModel.DataAnnotations;

namespace ProductService.Models;

/// <summary>
/// Request body for POST api/products.
/// </summary>
public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    // DECIMAL(18,2): at least 0.01, at most 16 digits before the decimal point.
    [Range(0.01, 9999999999999999.99, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "StockQuantity cannot be negative.")]
    public int StockQuantity { get; set; }
}
