using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class ProductCategoryService(IProductCategoryRepository productCategoryRepository)
{
    public async Task<PagedResult<GetProductCategory>> SearchAsync(PaginationQuery query)
    {
        return await productCategoryRepository.SearchAsync(query);
    }

    public async Task<GetProductCategory> GetByIdAsync(int id)
    {
        return await productCategoryRepository.GetDetailByIdAsync(id) ?? throw new NotFoundException("Product category not found.");
    }

    public async Task<GetProductCategory> CreateAsync(UpsertProductCategoryRequest request)
    {
        var productCategory = new ProductCategory
        {
            Name = request.Name
        };

        await productCategoryRepository.AddAsync(productCategory);

        return await GetByIdAsync(productCategory.Id);
    }

    public async Task<GetProductCategory> UpdateAsync(int id, UpsertProductCategoryRequest request)
    {
        var productCategory = await productCategoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product category not found.");

        productCategory.Name = request.Name;

        await productCategoryRepository.UpdateAsync(productCategory);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var productCategory = await productCategoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product category not found.");

        await productCategoryRepository.DeleteAsync(productCategory);
    }
}
