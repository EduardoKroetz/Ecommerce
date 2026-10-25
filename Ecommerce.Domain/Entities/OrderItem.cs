namespace Ecommerce.Domain.Entities;

public class OrderItem : BaseEntity
{
    private OrderItem() { ProductName = null!; } // EF

    // Snapshot of the product at purchase time (ADR 0003): price and name are frozen
    public OrderItem(Product product, int quantity)
    {
        ProductId = product.Id;
        ProductName = product.Name;
        UnitPrice = product.Price;
        Quantity = quantity;
        TotalPrice = product.Price * quantity;
    }

    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }
}
