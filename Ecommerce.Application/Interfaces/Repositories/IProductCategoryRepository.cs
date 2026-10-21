using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IProductCategoryRepository
{
    Task<PagedResult<ProductCategoryResponse>> SearchAsync(PaginationQuery query);
    Task<ProductCategoryResponse?> GetDetailByIdAsync(int id);

    Task<ProductCategory?> GetByIdAsync(int id);
    Task<ProductCategory> AddAsync(ProductCategory productCategory);
    Task<ProductCategory> UpdateAsync(ProductCategory productCategory);
    Task DeleteAsync(ProductCategory productCategory);
}
