using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public async Task<PagedResult<GetProduct>> SearchAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
    {
        var productQuery = dbContext.Products
            .AsNoTracking()
            .Where(p =>
                (string.IsNullOrWhiteSpace(query.Name) || p.Name.ToLower().Contains(query.Name.ToLower())) &&
                (query.Categories == null || query.Categories.Count == 0 || query.Categories.Any(cId => p.Categories.Any(pc => pc.Id == cId))) &&
                (query.StartPrice == null || p.Price >= query.StartPrice.Value) &&
                (query.EndPrice == null || p.Price <= query.EndPrice.Value)
            );

        var orderedQuery = query.Sort switch
        {
            GetProductsQuery.SortByPriceDesc => productQuery.OrderByDescending(p => p.Price),
            GetProductsQuery.SortByNameAsc => productQuery.OrderBy(p => p.Name),
            GetProductsQuery.SortByPriceAsc => productQuery.OrderBy(p => p.Price),
            GetProductsQuery.SortByCreatedAtAsc => productQuery.OrderBy(p => p.CreatedAt),
            _ => productQuery.OrderByDescending(p => p.CreatedAt),
        };

        return await orderedQuery
            .ThenBy(p => p.Id)
            .ToPagedResultAsync(GetProduct.MapExpression, query);
    }

    public async Task<GetProduct?> GetDetailByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(GetProduct.MapExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAndSellerIdAsync(int id, int sellerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> HasOrdersAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderItems.AnyAsync(oi => oi.ProductId == id, cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<bool> TryDecrementStockAsync(int productId, int quantity)
    {
        var updatedRows = await dbContext.Products
            .Where(p => p.Id == productId && p.StockBalance >= quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockBalance, p => p.StockBalance - quantity));

        return updatedRows > 0;
    }
}
