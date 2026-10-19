using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class CartItem
{
    public const int MaxQuantity = 9999;

    // Used by EF Core when materializing; skips the quantity validation for rows already persisted
    private CartItem()
    {
    }

    public CartItem(int productId, int quantity)
    {
        ProductId = productId;
        SetQuantity(quantity);
    }

    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; private set; }

    public Product Product { get; set; } = null!;
    public Cart Cart { get; set; } = null!;

    public void IncreaseQuantity(int quantity)
    {
        // Compared as a subtraction so the sum can never overflow int
        if (quantity > MaxQuantity - Quantity)
            throw QuantityExceeded();

        Quantity += quantity;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity > MaxQuantity)
            throw QuantityExceeded();

        Quantity = quantity;
    }

    private static DomainException QuantityExceeded() =>
        new(EDomainExceptionCode.CartItemQuantityExceeded, $"Cart item quantity cannot exceed {MaxQuantity}.");
}
