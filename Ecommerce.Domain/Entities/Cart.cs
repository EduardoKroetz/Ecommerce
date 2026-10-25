using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class Cart : BaseEntity
{
    private Cart() { } // EF

    public Cart(int customerId, DateTime createdAt)
    {
        CustomerId = customerId;
        CreatedAt = createdAt;
    }

    public int CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public void AddItem(int productId, int quantity)
    {
        var item = _items.FirstOrDefault(ci => ci.ProductId == productId);

        if (item is null)
            _items.Add(new CartItem(productId, quantity));
        else
            item.IncreaseQuantity(quantity);
    }

    public void UpdateItemQuantity(int productId, int quantity)
    {
        GetItem(productId).SetQuantity(quantity);
    }

    public void RemoveItem(int productId)
    {
        _items.Remove(GetItem(productId));
    }

    public void Clear()
    {
        _items.Clear();
    }

    public void EnsureNotEmpty()
    {
        if (_items.Count == 0)
            throw new DomainException(EDomainExceptionCode.CartEmpty, "Cart cannot be empty.");
    }

    private CartItem GetItem(int productId) =>
        _items.FirstOrDefault(ci => ci.ProductId == productId)
            ?? throw new DomainException(EDomainExceptionCode.CartItemNotFound, "Cart item not found in cart.");
}
