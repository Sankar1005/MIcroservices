namespace OrderService.Api.Models;

/// <summary>
/// One row of ordering.Orders plus its lines from ordering.OrderItems.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}
