using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Ecommerce.Infra.Projections;
using Ecommerce.Infra.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

internal class ProductRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider, ProductProjections productProjections) : IProductRepository
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
            .ToPagedResultAsync(productProjections.ToResponse, query, cancellationTokenProvider.Token);
    }

    public async Task<ProductResponse?> GetDetailByIdAsync(int id)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(productProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationTokenProvider.Token);
    }

    public async Task<Product?> GetByIdForSellerAsync(int id, int sellerId)
    {
        return await dbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId, cancellationTokenProvider.Token);
    }

    public async Task<List<Product>> GetListByIdsAsync(IEnumerable<int> ids)
    {
        var productIds = ids.Distinct().ToList();

        return await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationTokenProvider.Token);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await dbContext.Products.AnyAsync(p => p.Id == id, cancellationTokenProvider.Token);
    }

    public async Task<bool> ExistsForSellerAsync(int id, int sellerId)
    {
        return await dbContext.Products.AnyAsync(p => p.Id == id && p.SellerId == sellerId, cancellationTokenProvider.Token);
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

        // Stock is only written by the atomic operations below; never overwrite it with the value loaded earlier
        dbContext.Entry(product).Property(p => p.StockBalance).IsModified = false;

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

    public async Task IncrementStockAsync(int productId, int quantity)
    {
        await dbContext.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockBalance, p => p.StockBalance + quantity));
    }

    public async Task<bool> TryAdjustStockAsync(int productId, int quantity)
    {
        var updatedRows = await dbContext.Products
            .Where(p => p.Id == productId && p.StockBalance + quantity >= 0)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockBalance, p => p.StockBalance + quantity));

        return updatedRows > 0;
    }
}
