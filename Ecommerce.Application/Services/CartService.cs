using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Application.Extensions;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class CartService(
    ICartRepository cartRepository,
    ICurrentUserProvider userProvider,
    IProductRepository productRepository,
    TimeProvider timeProvider)
{
    private async Task<Cart> CreateCartAsync()
    {
        var cart = new Cart(userProvider.CustomerId, timeProvider.GetUtcDateTime());

        await cartRepository.AddAsync(cart);
        return cart;
    }

    // Product names come from another aggregate, so the response is always read back through the projection
    private async Task<CartResponse> GetCurrentCartDetailAsync()
    {
        return await cartRepository.GetDetailForCustomerAsync(userProvider.CustomerId)
            ?? throw new InvalidOperationException("Cart must exist at this point.");
    }

    private async Task<Cart> GetOrCreateCartAsync()
    {
        return await cartRepository.GetForCustomerAsync(userProvider.CustomerId)
            ?? await CreateCartAsync();
    }

    public async Task<CartResponse> GetCartDetailAsync()
    {
        var cartDetail = await cartRepository.GetDetailForCustomerAsync(userProvider.CustomerId);
        if (cartDetail is not null)
            return cartDetail;

        await CreateCartAsync();

        return await GetCurrentCartDetailAsync();
    }

    public async Task<CartResponse> AddToCartAsync(AddCartItemRequest request)
    {
        if (!await productRepository.ExistsAsync(request.ProductId))
            throw new NotFoundException("Product not found.");

        var cart = await GetOrCreateCartAsync();

        cart.AddItem(request.ProductId, request.Quantity);

        await cartRepository.UpdateAsync(cart);

        return await GetCurrentCartDetailAsync();
    }

    public async Task<CartResponse> RemoveFromCartAsync(int productId)
    {
        var cart = await GetOrCreateCartAsync();

        cart.RemoveItem(productId);

        await cartRepository.UpdateAsync(cart);

        return await GetCurrentCartDetailAsync();
    }

    public async Task<CartResponse> UpdateCartItemAsync(int productId, UpdateCartItemRequest request)
    {
        var cart = await GetOrCreateCartAsync();

        cart.UpdateItemQuantity(productId, request.Quantity);

        await cartRepository.UpdateAsync(cart);

        return await GetCurrentCartDetailAsync();
    }
}
