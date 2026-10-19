using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PurchasesController(PurchaseService purchaseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPurchases([FromQuery] PaginationQuery query)
    {
        var purchases = await purchaseService.SearchAsync(query);

        return Ok(purchases);
    }

    [HttpGet("{purchaseId:int}")]
    public async Task<IActionResult> GetPurchaseById(int purchaseId)
    {
        var purchase = await purchaseService.GetByIdAsync(purchaseId);

        return Ok(purchase);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseRequest request)
    {
        var purchase = await purchaseService.CreateAsync(request);

        return CreatedAtAction(
            actionName: nameof(GetPurchaseById),
            routeValues: new { purchaseId = purchase.Id },
            value: purchase);
    }

    [HttpPost("{purchaseId:int}/cancel")]
    public async Task<IActionResult> CancelPurchase(int purchaseId)
    {
        await purchaseService.CancelAsync(purchaseId);

        return NoContent();
    }

    [HttpPost("{purchaseId:int}/pay")]
    public async Task<IActionResult> PayPurchase(int purchaseId)
    {
        await purchaseService.PayAsync(purchaseId);

        return NoContent();
    }
}
