using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IProductCategoryRepository
{
    Task<PagedResult<ProductCategoryResponse>> SearchAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ProductCategoryResponse?> GetDetailByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductCategory> AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    Task<ProductCategory> UpdateAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
}
