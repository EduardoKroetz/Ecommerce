using Ecommerce.Api.Authorization;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] PaginationQuery query)
    {
        var orders = await orderService.SearchAsync(query);

        return Ok(orders);
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        var order = await orderService.GetByIdAsync(orderId);

        return Ok(order);
    }

    [HttpPost("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        await orderService.CancelAsync(orderId);

        return NoContent();
    }

    [HttpPost("{orderId:int}/ship")]
    [Authorize(Policy = Policies.RequireSeller)]
    public async Task<IActionResult> ShipOrder(int orderId)
    {
        await orderService.ShipAsync(orderId);

        return NoContent();
    }

    [HttpPost("{orderId:int}/confirm-delivery")]
    public async Task<IActionResult> ConfirmDelivery(int orderId)
    {
        await orderService.ConfirmDeliveryAsync(orderId);

        return NoContent();
    }
}
