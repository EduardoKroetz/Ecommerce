using Ecommerce.Data;
using Ecommerce.DTOs.Products;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
public class ProductsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await dbContext.Products.FindAsync(id);

        if (product == null) return NotFound();

        return Ok(GetProduct.Map(product));
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        var queryable = dbContext.Products
            .Include(p => p.Categories)
            .Where(p =>
                (string.IsNullOrEmpty(query.Name) || p.Name.Contains(query.Name)) &&
                (query.Categories.Count == 0 || query.Categories.Any(cId => p.Categories.Any(pc => pc.Id == cId))) &&
                (query.StartPrice.HasValue && p.Price >= query.StartPrice.Value) &&
                (query.EndPrice.HasValue && p.Price <= query.EndPrice.Value)
            )
            .Select(p => GetProduct.Map(p))
            .AsQueryable();

        if (query.Sort == GetProductsQuery.SORT_BY_PRICE_DESC)
        {
            queryable = queryable.OrderByDescending(p => p.Price);
        }
        else if (query.Sort == GetProductsQuery.SORT_BY_NAME_ASC)
        {
            queryable = queryable.OrderBy(p => p.Name);
        }
        else if (query.Sort == GetProductsQuery.SORT_BY_PRICE_ASC)
        {
            queryable = queryable.OrderBy(p => p.Price);
        }
        else if (query.Sort == GetProductsQuery.SORT_BY_CREATED_AT_ASC)
        {
            queryable = queryable.OrderBy(p => p.CreatedAt);
        }

        var products = await queryable
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] UpsertProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockBalance = request.StockBalance,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpsertProductRequest request)
    {
        var product = await dbContext.Products.FindAsync(id);

        if (product == null) return NotFound();

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockBalance = request.StockBalance;

        await dbContext.SaveChangesAsync();

        return Ok(GetProduct.Map(product));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await dbContext.Products.FindAsync(id);

        if (product == null) return NotFound();

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();

        return Ok();
    }
}
