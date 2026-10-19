using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class ProductCategoryRepository(AppDbContext dbContext) : IProductCategoryRepository
{
    public async Task<PagedResult<ProductCategoryResponse>> SearchAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var productCategoryQuery = dbContext.ProductCategories
            .AsNoTracking()
            .OrderBy(pc => pc.Name)
            .ThenBy(pc => pc.Id);

        return await productCategoryQuery.ToPagedResultAsync(ProductCategoryResponse.MapExpression, query);
    }

    public async Task<ProductCategoryResponse?> GetDetailByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .Where(pc => pc.Id == id)
            .Select(ProductCategoryResponse.MapExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductCategories.FindAsync([id], cancellationToken);
    }

    public async Task<ProductCategory> AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
    {
        await dbContext.ProductCategories.AddAsync(productCategory, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return productCategory;
    }

    public async Task<ProductCategory> UpdateAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
    {
        dbContext.ProductCategories.Update(productCategory);
        await dbContext.SaveChangesAsync(cancellationToken);
        return productCategory;
    }

    public async Task DeleteAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
    {
        dbContext.ProductCategories.Remove(productCategory);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
