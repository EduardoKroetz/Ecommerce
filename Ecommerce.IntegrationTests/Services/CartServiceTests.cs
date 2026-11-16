using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Exceptions;
using Ecommerce.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.IntegrationTests.Services;

public class CartServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCartDetail_WithoutCart_CreatesEmptyCart()
    {
        var created = await Services.Get<CartService>().GetCartDetailAsync();

        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        Assert.Empty(cart.Items);
        Assert.Equal(created.Id, cart.Id);
    }

    [Fact]
    public async Task GetCartDetail_ReturnsOnlyCurrentCustomerCart()
    {
        var otherCustomer = await TestData.CustomerAsync();
        var product = await TestData.ProductAsync();
        await TestData.CartItemAsync(product: product);
        await TestData.CartItemAsync(otherCustomer);

        var cart = await Services.Get<CartService>().GetCartDetailAsync();

        Assert.Equal(product.Id, Assert.Single(cart.Items).ProductId);
    }

    [Fact]
    public async Task GetCartDetail_ShowsCurrentProductName()
    {
        var product = await TestData.ProductAsync(name: "Old Name");
        await TestData.CartItemAsync(product: product);
        await Services.Get<ProductService>(Seller).UpdateAsync(product.Id, new UpdateProductRequest { Name = "New Name", Price = product.Price });

        var cart = await Services.Get<CartService>().GetCartDetailAsync();

        Assert.Equal("New Name", Assert.Single(cart.Items).ProductName);
    }

    [Fact]
    public async Task AddToCart_WithoutCart_CreatesCartWithItem()
    {
        var product = await TestData.ProductAsync();

        await Services.Get<CartService>().AddToCartAsync(new AddCartItemRequest { ProductId = product.Id, Quantity = 2 });

        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        var item = Assert.Single(cart.Items);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public async Task AddToCart_WithProductAlreadyInCart_PersistsSummedQuantity()
    {
        var product = await TestData.ProductAsync();
        await TestData.CartItemAsync(product: product, quantity: 2);

        await Services.Get<CartService>().AddToCartAsync(new AddCartItemRequest { ProductId = product.Id, Quantity = 3 });

        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        Assert.Equal(5, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public async Task AddToCart_WithUnknownProduct_IsNotFound()
    {
        var service = Services.Get<CartService>();

        await Assert.ThrowsAsync<NotFoundException>(() => service.AddToCartAsync(new AddCartItemRequest { ProductId = 999999, Quantity = 1 }));

        var cartCount = await QueryAsync(db => db.Carts.CountAsync(c => c.CustomerId == Customer.CustomerId));
        Assert.Equal(0, cartCount);
    }

    [Fact]
    public async Task UpdateCartItem_PersistsQuantity()
    {
        var product = await TestData.ProductAsync();
        await TestData.CartItemAsync(product: product, quantity: 5);

        await Services.Get<CartService>().UpdateCartItemAsync(product.Id, new UpdateCartItemRequest { Quantity = 2 });

        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        Assert.Equal(2, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public async Task UpdateCartItem_WithoutCart_IsRejected()
    {
        var product = await TestData.ProductAsync();
        var service = Services.Get<CartService>();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.UpdateCartItemAsync(product.Id, new UpdateCartItemRequest { Quantity = 2 }));
        Assert.Equal(EDomainExceptionCode.CartItemNotFound, exception.Code);

        var cartCount = await QueryAsync(db => db.Carts.CountAsync(c => c.CustomerId == Customer.CustomerId));
        Assert.Equal(0, cartCount);
    }

    [Fact]
    public async Task RemoveFromCart_RemovesOnlyThatItem()
    {
        var removed = await TestData.ProductAsync();
        var kept = await TestData.ProductAsync();
        await TestData.CartItemAsync(product: removed);
        await TestData.CartItemAsync(product: kept);

        await Services.Get<CartService>().RemoveFromCartAsync(removed.Id);

        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        Assert.Equal(kept.Id, Assert.Single(cart.Items).ProductId);
    }

    [Fact]
    public async Task RemoveFromCart_WithoutCart_IsRejected()
    {
        var product = await TestData.ProductAsync();
        var service = Services.Get<CartService>();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.RemoveFromCartAsync(product.Id));
        Assert.Equal(EDomainExceptionCode.CartItemNotFound, exception.Code);

        var cartCount = await QueryAsync(db => db.Carts.CountAsync(c => c.CustomerId == Customer.CustomerId));
        Assert.Equal(0, cartCount);
    }
}
