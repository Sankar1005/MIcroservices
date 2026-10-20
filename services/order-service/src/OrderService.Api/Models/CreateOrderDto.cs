using System.ComponentModel.DataAnnotations;

namespace OrderService.Api.Models;

/// <summary>
/// Request body for POST api/orders. The client sends only product ids and quantities;
/// names and prices are fetched from ProductService (a client must not decide the price).
/// </summary>
public class CreateOrderDto
{
    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "An order needs at least one item.")]
    [MaxLength(50, ErrorMessage = "An order can have at most 50 items.")]
    public List<CreateOrderItemDto> Items { get; set; } = [];
}

public class CreateOrderItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive number.")]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
    public int Quantity { get; set; }
}
