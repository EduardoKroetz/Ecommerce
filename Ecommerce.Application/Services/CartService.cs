using Ecommerce.Application.DTOs.CartItems;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class CartService(ICartRepository cartRepository, ICurrentUserProvider userProvider, IProductRepository productRepository)
{
    private async Task<Cart> CreateCartAsync()
    {
        var userId = userProvider.UserId;

        var cart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await cartRepository.AddAsync(cart);
        return cart;
    }

    private async Task<Cart> GetOrCreateCartAsync()
    {
        var userId = userProvider.UserId;

        var cart = await cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = await CreateCartAsync();
        }

        return cart;
    }

    public async Task<GetCart> GetCartDetailAsync()
    {
        var userId = userProvider.UserId;

        var cartDetail = await cartRepository.GetDetailByUserIdAsync(userId);
        if (cartDetail is null)
        {
            var cart = await CreateCartAsync();

            cartDetail = GetCart.Map(cart);
        }

        return cartDetail;
    }

    public async Task<GetCart> AddToCartAsync(AddToCartRequest request)
    {
        var userId = userProvider.UserId;

        if (!await productRepository.ExistsAsync(request.ProductId))
            throw new NotFoundException($"Product not found.");

        var cart = await GetOrCreateCartAsync();

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == request.ProductId);

        if (cartItem != null)
        {
            cartItem.Quantity += request.Quantity;
        }
        else
        {
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            cart.Items.Add(cartItem);
        }

        await cartRepository.UpdateAsync(cart);

        return GetCart.Map(cart);
    }

    public async Task<GetCart> RemoveFromCartAsync(int productId)
    {
        var userId = userProvider.UserId;

        var cart = await cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = await CreateCartAsync();
        }

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item not found in cart.");

        cart.Items.Remove(cartItem);
        await cartRepository.UpdateAsync(cart);

        return GetCart.Map(cart);
    }

    public async Task<GetCart> UpdateCartItemAsync(int productId, UpdateCartItemRequest request)
    {
        var userId = userProvider.UserId;

        var cart = await cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = await CreateCartAsync();
        }

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item not found in cart.");

        cartItem.Quantity = request.Quantity;
        await cartRepository.UpdateAsync(cart);

        return GetCart.Map(cart);
    }
}
