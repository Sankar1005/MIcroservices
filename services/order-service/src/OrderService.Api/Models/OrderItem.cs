namespace OrderService.Api.Models;

/// <summary>
/// One row of ordering.OrderItems. ProductName and UnitPrice are copies taken from ProductService
/// at the moment the order was placed (a "snapshot").
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
