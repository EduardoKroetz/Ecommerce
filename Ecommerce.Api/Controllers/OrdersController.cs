using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OrdersController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {


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
