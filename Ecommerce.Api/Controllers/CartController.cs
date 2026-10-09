using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class CartController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("items")]
    public async Task<IActionResult> GetCartItems([FromQuery] PaginationRequest paginationRequest)
    {
        var userId = User.GetUserId();

        var cartItems = await dbContext.CartItems
            .AsNoTracking()
            .Where(ci => ci.Cart.UserId == userId)
            .Select(GetCartItem.MapExpression)
            .Skip(paginationRequest.Skip)
            .Take(paginationRequest.Take)
            .ToListAsync();

        return Ok(cartItems);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart(AddToCartRequest request)
    {
        var userId = User.GetUserId();

        if (!await dbContext.Products.AnyAsync(p => p.Id == request.ProductId))
            return Problem(title: "Product not found.", statusCode: StatusCodes.Status404NotFound);

        var cart = await dbContext.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.Carts.AddAsync(cart);
            await dbContext.SaveChangesAsync();
        }

        var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.CartId == cart.Id
            && ci.ProductId == request.ProductId);

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

            await dbContext.CartItems.AddAsync(cartItem);
        }

        await dbContext.SaveChangesAsync();

        var cartItemResponse = await dbContext.CartItems
            .Where(ci => ci.Id == cartItem.Id)
            .Select(GetCartItem.MapExpression)
            .FirstOrDefaultAsync();

        return Ok(cartItemResponse);
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var userId = User.GetUserId();

        var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.Cart.UserId == userId
            && ci.ProductId == productId);

        if (cartItem == null)
        {
            return Problem(title: "Item not found in cart.", statusCode: StatusCodes.Status404NotFound);
        }

        dbContext.CartItems.Remove(cartItem);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateCartItem(int productId, UpdateCartItemRequest request)
    {
        var userId = User.GetUserId();

        var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.Cart.UserId == userId
            && ci.ProductId == productId);

        if (cartItem == null)
        {
            return Problem(title: "Item not found in cart.", statusCode: StatusCodes.Status404NotFound);
        }

        cartItem.Quantity = request.Quantity;

        dbContext.CartItems.Update(cartItem);
        await dbContext.SaveChangesAsync();

        var cartItemResponse = await dbContext.CartItems
            .Where(ci => ci.Id == cartItem.Id)
            .Select(GetCartItem.MapExpression)
            .FirstOrDefaultAsync();

        return Ok(cartItemResponse);
    }
}
