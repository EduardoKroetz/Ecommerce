using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IProductCategoryRepository
{
    Task<PagedResult<GetProductCategory>> SearchAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<GetProductCategory?> GetDetailByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductCategory> AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    Task<ProductCategory> UpdateAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
}
