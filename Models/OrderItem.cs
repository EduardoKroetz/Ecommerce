namespace Ecommerce.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public required int ProductId { get; set; }
    public required string ProductName { get; set; }
    public required decimal UnitPrice { get; set; }
    public required int Quantity { get; set; }
    public required decimal TotalPrice { get; set; }

    public Product Product { get; set; } = null!;
}
