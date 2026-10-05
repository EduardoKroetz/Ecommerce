using Ecommerce.Data;
using Ecommerce.DTOs;
using Ecommerce.DTOs.ProductCategories;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ProductCategoriesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductCategoryById(int id)
    {
        var productCategory = await dbContext.ProductCategories
            .AsNoTracking()
            .Select(GetProductCategory.MapExpression)
            .FirstOrDefaultAsync(pc => pc.Id == id);

        if (productCategory == null) return NotFound();

        return Ok(productCategory);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductCategories([FromQuery] PaginationRequest query)
    {
        var products = await dbContext.ProductCategories
            .AsNoTracking()
            .Select(GetProductCategory.MapExpression)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductCategory([FromBody] UpsertProductCategoryRequest request)
    {
        var productCategory = new ProductCategory
        {
            Name = request.Name
        };

        dbContext.ProductCategories.Add(productCategory);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(
            actionName: nameof(GetProductCategoryById),
            routeValues: new { id = productCategory.Id },
            value: null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductCategory(int id, [FromBody] UpsertProductCategoryRequest request)
    {
        var productCategory = await dbContext.ProductCategories.FindAsync(id);

        if (productCategory == null) return NotFound();

        productCategory.Name = request.Name;

        await dbContext.SaveChangesAsync();

        var productCategoryResponse = await dbContext.ProductCategories
            .Select(GetProductCategory.MapExpression)
            .FirstOrDefaultAsync(pc => pc.Id == id);

        return Ok(productCategoryResponse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductCategory(int id)
    {
        var productCategory = await dbContext.ProductCategories.FindAsync(id);

        if (productCategory == null) return NotFound();

        dbContext.ProductCategories.Remove(productCategory);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
