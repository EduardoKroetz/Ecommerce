using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Ecommerce.Infra.Projections;
using Ecommerce.Infra.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

internal class ProductCategoryRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider, ProductCategoryProjections productCategoryProjections) : IProductCategoryRepository
{
    public async Task<PagedResult<ProductCategoryResponse>> SearchAsync(PaginationQuery query)
    {
        var productCategoryQuery = dbContext.ProductCategories
            .AsNoTracking()
            .OrderBy(pc => pc.Name)
            .ThenBy(pc => pc.Id);

        return await productCategoryQuery.ToPagedResultAsync(productCategoryProjections.ToResponse, query, cancellationTokenProvider.Token);
    }

    public async Task<ProductCategoryResponse?> GetDetailByIdAsync(int id)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .Where(pc => pc.Id == id)
            .Select(productCategoryProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationTokenProvider.Token);
    }

    public async Task<ProductCategory?> GetByIdAsync(int id)
    {
        return await dbContext.ProductCategories.FindAsync([id], cancellationTokenProvider.Token);
    }

    public async Task<ProductCategory> AddAsync(ProductCategory productCategory)
    {
        await dbContext.ProductCategories.AddAsync(productCategory);
        await dbContext.SaveChangesAsync();
        return productCategory;
    }

    public async Task<ProductCategory> UpdateAsync(ProductCategory productCategory)
    {
        dbContext.ProductCategories.Update(productCategory);
        await dbContext.SaveChangesAsync();
        return productCategory;
    }

    public async Task DeleteAsync(ProductCategory productCategory)
    {
        dbContext.ProductCategories.Remove(productCategory);
        await dbContext.SaveChangesAsync();
    }
}
