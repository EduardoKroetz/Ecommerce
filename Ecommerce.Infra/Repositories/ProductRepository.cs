using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Services;
using Ecommerce.Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class ProductRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider) : IProductRepository
{
    public async Task<PagedResult<ProductResponse>> SearchAsync(SearchProductsQuery query)
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
            SearchProductsQuery.SortByPriceDesc => productQuery.OrderByDescending(p => p.Price),
            SearchProductsQuery.SortByNameAsc => productQuery.OrderBy(p => p.Name),
            SearchProductsQuery.SortByPriceAsc => productQuery.OrderBy(p => p.Price),
            SearchProductsQuery.SortByCreatedAtAsc => productQuery.OrderBy(p => p.CreatedAt),
            _ => productQuery.OrderByDescending(p => p.CreatedAt),
        };

        return await orderedQuery
            .ThenBy(p => p.Id)
            .ToPagedResultAsync(ProductResponse.MapExpression, query, cancellationTokenProvider.Token);
    }

    public async Task<ProductResponse?> GetDetailByIdAsync(int id)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ProductResponse.MapExpression)
            .FirstOrDefaultAsync(cancellationTokenProvider.Token);
    }

    public async Task<Product?> GetByIdForSellerAsync(int id, int sellerId)
    {
        return await dbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId, cancellationTokenProvider.Token);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await dbContext.Products.AnyAsync(p => p.Id == id, cancellationTokenProvider.Token);
    }

    public async Task<bool> HasOrdersAsync(int id)
    {
        return await dbContext.OrderItems.AnyAsync(oi => oi.ProductId == id, cancellationTokenProvider.Token);
    }

    public async Task<Product> AddAsync(Product product)
    {
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(Product product)
    {
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync();
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
