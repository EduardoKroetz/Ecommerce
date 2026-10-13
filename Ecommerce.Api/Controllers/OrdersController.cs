using Ecommerce.Application.DTOs;
using Ecommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var order = await orderService.CreateAsync();

        return Ok(order);
    }

    [HttpPatch("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        await orderService.CancelAsync(orderId);

        return NoContent();
    }

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
}
