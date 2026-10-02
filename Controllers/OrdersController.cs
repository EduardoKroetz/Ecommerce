using Ecommerce.Data;
using Ecommerce.DTOs;
using Ecommerce.DTOs.Orders;
using Ecommerce.Enums;
using Ecommerce.Extensions;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OrdersController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var userId = User.GetUserId();

        var cartItems = await dbContext.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.Cart.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return Problem(title: "Cart is empty.", statusCode: StatusCodes.Status400BadRequest);
        }

        var itemsWithInsufficientStock = cartItems
            .Where(ci => ci.Quantity > ci.Product.StockBalance)
            .Select(ci => new { ci.ProductId, ci.Product.Name, ci.Product.StockBalance, ci.Quantity })
            .ToList();

        if (itemsWithInsufficientStock.Count > 0)
        {
            return Problem(
                title: "Insufficient stock",
                detail: "Some items in your cart have insufficient stock.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    ["items"] = itemsWithInsufficientStock
                });
        }

        var orderItems = cartItems.Select(ci => new OrderItem
        {
            ProductId = ci.ProductId,
            ProductName = ci.Product.Name,
            Quantity = ci.Quantity,
            UnitPrice = ci.Product.Price,
            TotalPrice = ci.Product.Price * ci.Quantity,
        }).ToList();

        var order = new Order
        {
            UserId = userId,
            Status = EOrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = orderItems.Sum(oi => oi.TotalPrice),
            Items = orderItems
        };

        await dbContext.Orders.AddAsync(order);

        dbContext.CartItems.RemoveRange(cartItems);

        cartItems.ForEach(ci => ci.Product.StockBalance -= ci.Quantity);

        await dbContext.SaveChangesAsync();

        return Ok(new { OrderId = order.Id });
    }

    [HttpPatch("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var userId = User.GetUserId();

        var order = await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId && o.Id == orderId)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return Problem(title: "Order not found.", statusCode: StatusCodes.Status404NotFound);
        }

        if (order.Status == EOrderStatus.Cancelled)
        {
            return Problem(title: "Order is already cancelled.", statusCode: StatusCodes.Status400BadRequest);
        }

        order.Status = EOrderStatus.Cancelled;

        order.Items.ForEach(oi => oi.Product.StockBalance += oi.Quantity);

        await dbContext.SaveChangesAsync();

        return Ok(new { OrderId = order.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] PaginationRequest request)
    {
        var userId = User.GetUserId();

        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Select(GetOrder.MapExpression)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        var userId = User.GetUserId();

        var order = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId && o.Id == orderId)
            .Select(GetOrder.MapExpression)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return Problem(title: "Order not found.", statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(order);
    }
}
