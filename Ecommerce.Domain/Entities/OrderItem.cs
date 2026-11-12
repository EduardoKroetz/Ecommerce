using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class OrderItem : BaseEntity
{
    private OrderItem() { ProductName = null!; } // EF

    // Snapshot of the product at purchase time (ADR 0003): price and name are frozen
    public OrderItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(EDomainExceptionCode.InvalidOrderItemQuantity, "Order item quantity must be greater than zero.");

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        TotalPrice = unitPrice * quantity;
    }

    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }
}
