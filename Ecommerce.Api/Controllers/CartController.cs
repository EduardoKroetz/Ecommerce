using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class CartController(CartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCartDetailAsync()
    {
        var cart = await cartService.GetCartDetailAsync();

        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart(AddCartItemRequest request)
    {
        var cart = await cartService.AddToCartAsync(request);

        return Ok(cart);
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var cart = await cartService.RemoveFromCartAsync(productId);

        return Ok(cart);
    }

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateCartItem(int productId, UpdateCartItemRequest request)
    {
        var cart = await cartService.UpdateCartItemAsync(productId, request);

        return Ok(cart);
    }
}
