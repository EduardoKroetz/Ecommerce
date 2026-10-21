using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class CartService(ICartRepository cartRepository, ICurrentUserProvider userProvider, IProductRepository productRepository)
{
    private async Task<Cart> CreateCartAsync()
    {
        var cart = new Cart
        {
            CustomerId = userProvider.CustomerId,
            CreatedAt = DateTime.UtcNow
        };

        await cartRepository.AddAsync(cart);
        return cart;
    }

    private async Task<Cart> GetOrCreateCartAsync()
    {
        var cart = await cartRepository.GetForCustomerAsync(userProvider.CustomerId);
        if (cart == null)
        {
            cart = await CreateCartAsync();
        }

        return cart;
    }

    public async Task<CartResponse> GetCartDetailAsync()
    {
        var cartDetail = await cartRepository.GetDetailForCustomerAsync(userProvider.CustomerId);
        if (cartDetail is null)
        {
            var cart = await CreateCartAsync();

            cartDetail = CartResponse.Map(cart);
        }

        return cartDetail;
    }

    public async Task<CartResponse> AddToCartAsync(AddCartItemRequest request)
    {
        if (!await productRepository.ExistsAsync(request.ProductId))
            throw new NotFoundException($"Product not found.");

        var cart = await GetOrCreateCartAsync();

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == request.ProductId);

        if (cartItem != null)
        {
            cartItem.IncreaseQuantity(request.Quantity);
        }
        else
        {
            cartItem = new CartItem(request.ProductId, request.Quantity)
            {
                CartId = cart.Id
            };

            cart.Items.Add(cartItem);
        }

        await cartRepository.UpdateAsync(cart);

        return (await cartRepository.GetDetailForCustomerAsync(userProvider.CustomerId))!;
    }

    public async Task<CartResponse> RemoveFromCartAsync(int productId)
    {
        var cart = await cartRepository.GetForCustomerAsync(userProvider.CustomerId);
        if (cart == null)
        {
            cart = await CreateCartAsync();
        }

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item not found in cart.");

        cart.Items.Remove(cartItem);
        await cartRepository.UpdateAsync(cart);

        return CartResponse.Map(cart);
    }

    public async Task<CartResponse> UpdateCartItemAsync(int productId, UpdateCartItemRequest request)
    {
        var cart = await cartRepository.GetForCustomerAsync(userProvider.CustomerId);
        if (cart == null)
        {
            cart = await CreateCartAsync();
        }

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item not found in cart.");

        cartItem.SetQuantity(request.Quantity);
        await cartRepository.UpdateAsync(cart);

        return CartResponse.Map(cart);
    }
}
