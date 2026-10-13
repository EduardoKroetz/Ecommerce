using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ProductCategoriesController(ProductCategoryService productCategoryService) : ControllerBase
{
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductCategoryById(int id)
    {
        var productCategory = await productCategoryService.GetByIdAsync(id);

        return Ok(productCategory);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductCategories([FromQuery] PaginationQuery query)
    {
        var productCategories = await productCategoryService.SearchAsync(query);

        return Ok(productCategories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductCategory([FromBody] UpsertProductCategoryRequest request)
    {
        var productCategory = await productCategoryService.CreateAsync(request);

        return CreatedAtAction(
            actionName: nameof(GetProductCategoryById),
            routeValues: new { id = productCategory.Id },
            value: productCategory);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductCategory(int id, [FromBody] UpsertProductCategoryRequest request)
    {
        var productCategory = await productCategoryService.UpdateAsync(id, request);

        return Ok(productCategory);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductCategory(int id)
    {
        await productCategoryService.DeleteAsync(id);

        return NoContent();
    }
}
