using Ecommerce.Api.Authorization;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await productService.GetByIdAsync(id);

        return Ok(product);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts([FromQuery] SearchProductsQuery query)
    {
        var products = await productService.SearchAsync(query);

        return Ok(products);
    }

    [HttpPost]
    [Authorize(Policy = Policies.RequireSeller)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = await productService.CreateAsync(request);

        return CreatedAtAction(
            actionName: nameof(GetProductById),
            routeValues: new { id = product.Id },
            value: product);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.RequireSeller)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await productService.UpdateAsync(id, request);

        return Ok(product);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.RequireSeller)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await productService.DeleteAsync(id);

        return NoContent();
    }

    [HttpPost("{productId:int}/categories/{productCategoryId:int}")]
    [Authorize(Policy = Policies.RequireSeller)]
    public async Task<IActionResult> AddProductCategory([FromRoute] int productId, [FromRoute] int productCategoryId)
    {
        await productService.AddCategoryAsync(productId, productCategoryId);

        return NoContent();
    }

    [HttpDelete("{productId:int}/categories/{productCategoryId:int}")]
    [Authorize(Policy = Policies.RequireSeller)]
    public async Task<IActionResult> RemoveProductCategory([FromRoute] int productId, [FromRoute] int productCategoryId)
    {
        await productService.RemoveCategoryAsync(productId, productCategoryId);

        return NoContent();
    }
}
