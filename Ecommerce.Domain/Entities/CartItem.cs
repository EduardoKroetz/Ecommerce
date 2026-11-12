using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

// Part of the Cart aggregate: created and changed only through Cart
public class CartItem : BaseEntity
{
    public const int MinQuantity = 1;
    public const int MaxQuantity = 9999;

    private CartItem() { } // EF

    internal CartItem(int productId, int quantity)
    {
        ProductId = productId;
        SetQuantity(quantity);
    }

    public int CartId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }

    public Cart Cart { get; private set; } = null!;

    // Cross-aggregate navigation: Infra queries only, never commands or entity methods (domain-model.md)
    internal Product Product { get; private set; } = null!;

    internal void IncreaseQuantity(int quantity)
    {
        if (quantity < MinQuantity)
            throw InvalidQuantity();

        // Compared as a subtraction so the sum can never overflow int
        if (quantity > MaxQuantity - Quantity)
            throw QuantityExceeded();

        Quantity += quantity;
    }

    internal void SetQuantity(int quantity)
    {
        if (quantity < MinQuantity)
            throw InvalidQuantity();

        if (quantity > MaxQuantity)
            throw QuantityExceeded();

        Quantity = quantity;
    }

    private static DomainException InvalidQuantity() =>
        new(EDomainExceptionCode.InvalidCartItemQuantity, $"Cart item quantity must be at least {MinQuantity}.");

    private static DomainException QuantityExceeded() =>
        new(EDomainExceptionCode.CartItemQuantityExceeded, $"Cart item quantity cannot exceed {MaxQuantity}.");
}
