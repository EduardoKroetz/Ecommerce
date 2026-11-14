using Ecommerce.Domain.Exceptions;

namespace Ecommerce.UnitTests.Entities;

public class CartTests
{
    [Fact]
    public void Create_StartsEmpty()
    {
        var cart = CartTestData.Create();

        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Create_KeepsCustomerIdAndCreatedAt()
    {
        var createdAt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var cart = CartTestData.Create(customerId: 7, createdAt: createdAt);

        Assert.Equal(7, cart.CustomerId);
        Assert.Equal(createdAt, cart.CreatedAt);
    }

    [Fact]
    public void AddItem_WithNewProduct_AddsItem()
    {
        var cart = CartTestData.Create();

        cart.AddItem(productId: 1, quantity: 2);

        var item = Assert.Single(cart.Items);
        Assert.Equal(1, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void AddItem_WithProductAlreadyInCart_IncreasesQuantity()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 2);

        cart.AddItem(productId: 1, quantity: 3);

        var item = Assert.Single(cart.Items);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void AddItem_WithDifferentProducts_KeepsOneItemPerProduct()
    {
        var cart = CartTestData.Create();

        cart.AddItem(productId: 1, quantity: 2);
        cart.AddItem(productId: 2, quantity: 1);

        Assert.Equal(2, cart.Items.Count);
        Assert.Equal([1, 2], cart.Items.Select(i => i.ProductId).Order());
    }

    [Fact]
    public void AddItem_WithMaxQuantity_AddsItem()
    {
        var cart = CartTestData.Create();

        cart.AddItem(productId: 1, quantity: CartItem.MaxQuantity);

        Assert.Equal(CartItem.MaxQuantity, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void AddItem_WithQuantityAboveMax_IsRejected()
    {
        var cart = CartTestData.Create();

        var exception = Assert.Throws<DomainException>(() => cart.AddItem(productId: 1, quantity: CartItem.MaxQuantity + 1));
        Assert.Equal(EDomainExceptionCode.CartItemQuantityExceeded, exception.Code);
        Assert.Empty(cart.Items);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithNonPositiveQuantity_IsRejected(int quantity)
    {
        var cart = CartTestData.Create();

        var exception = Assert.Throws<DomainException>(() => cart.AddItem(productId: 1, quantity));
        Assert.Equal(EDomainExceptionCode.InvalidCartItemQuantity, exception.Code);
        Assert.Empty(cart.Items);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithNonPositiveQuantityForProductAlreadyInCart_IsRejected(int quantity)
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 5);

        var exception = Assert.Throws<DomainException>(() => cart.AddItem(productId: 1, quantity));
        Assert.Equal(EDomainExceptionCode.InvalidCartItemQuantity, exception.Code);
        Assert.Equal(5, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void AddItem_WhenSumReachesMax_IncreasesQuantity()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: CartItem.MaxQuantity - 1);

        cart.AddItem(productId: 1, quantity: 1);

        Assert.Equal(CartItem.MaxQuantity, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void AddItem_WhenSumExceedsMax_IsRejected()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: CartItem.MaxQuantity - 1);

        var exception = Assert.Throws<DomainException>(() => cart.AddItem(productId: 1, quantity: 2));
        Assert.Equal(EDomainExceptionCode.CartItemQuantityExceeded, exception.Code);
        Assert.Equal(CartItem.MaxQuantity - 1, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void AddItem_WhenSumWouldOverflow_IsRejected()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 1);

        var exception = Assert.Throws<DomainException>(() => cart.AddItem(productId: 1, quantity: int.MaxValue));
        Assert.Equal(EDomainExceptionCode.CartItemQuantityExceeded, exception.Code);
        Assert.Equal(1, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_ReplacesQuantity()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 5);

        cart.UpdateItemQuantity(productId: 1, quantity: 2);

        Assert.Equal(2, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_WithMaxQuantity_ReplacesQuantity()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 5);

        cart.UpdateItemQuantity(productId: 1, quantity: CartItem.MaxQuantity);

        Assert.Equal(CartItem.MaxQuantity, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_WithQuantityAboveMax_IsRejected()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 5);

        var exception = Assert.Throws<DomainException>(() => cart.UpdateItemQuantity(productId: 1, quantity: CartItem.MaxQuantity + 1));
        Assert.Equal(EDomainExceptionCode.CartItemQuantityExceeded, exception.Code);
        Assert.Equal(5, Assert.Single(cart.Items).Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateItemQuantity_WithNonPositiveQuantity_IsRejected(int quantity)
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 5);

        var exception = Assert.Throws<DomainException>(() => cart.UpdateItemQuantity(productId: 1, quantity));
        Assert.Equal(EDomainExceptionCode.InvalidCartItemQuantity, exception.Code);
        Assert.Equal(5, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_WithProductNotInCart_IsRejected()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 5);

        var exception = Assert.Throws<DomainException>(() => cart.UpdateItemQuantity(productId: 2, quantity: 3));
        Assert.Equal(EDomainExceptionCode.CartItemNotFound, exception.Code);
        Assert.Equal(5, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void RemoveItem_RemovesOnlyThatItem()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 2);
        cart.AddItem(productId: 2, quantity: 1);

        cart.RemoveItem(productId: 1);

        Assert.Equal(2, Assert.Single(cart.Items).ProductId);
    }

    [Fact]
    public void RemoveItem_WithProductNotInCart_IsRejected()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 2);

        var exception = Assert.Throws<DomainException>(() => cart.RemoveItem(productId: 2));
        Assert.Equal(EDomainExceptionCode.CartItemNotFound, exception.Code);
        Assert.Equal(1, Assert.Single(cart.Items).ProductId);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 2);
        cart.AddItem(productId: 2, quantity: 1);

        cart.Clear();

        Assert.Empty(cart.Items);
    }

    [Fact]
    public void EnsureNotEmpty_WhenEmpty_IsRejected()
    {
        var cart = CartTestData.Create();

        var exception = Assert.Throws<DomainException>(() => cart.EnsureNotEmpty());
        Assert.Equal(EDomainExceptionCode.CartEmpty, exception.Code);
    }

    [Fact]
    public void EnsureNotEmpty_WhenHasItems_DoesNotThrow()
    {
        var cart = CartTestData.Create();
        cart.AddItem(productId: 1, quantity: 1);

        var exception = Record.Exception(() => cart.EnsureNotEmpty());

        Assert.Null(exception);
    }
}
