using Ecommerce.Api.Authorization;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = Policies.RequireSeller)]
public class SellersController(OrderService orderService) : ControllerBase
{
    [HttpGet("me/orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] PaginationQuery query)
    {
        var orders = await orderService.SearchSellerOrdersAsync(query);

        return Ok(orders);
    }
}
