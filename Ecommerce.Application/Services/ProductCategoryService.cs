using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class ProductCategoryService(IProductCategoryRepository productCategoryRepository)
{
    public async Task<PagedResult<ProductCategoryResponse>> SearchAsync(PaginationQuery query)
    {
        return await productCategoryRepository.SearchAsync(query);
    }

    public async Task<ProductCategoryResponse> GetByIdAsync(int id)
    {
        return await productCategoryRepository.GetDetailByIdAsync(id) ?? throw new NotFoundException("Product category not found.");
    }

    public async Task<ProductCategoryResponse> CreateAsync(CreateProductCategoryRequest request)
    {
        var productCategory = new ProductCategory
        {
            Name = request.Name
        };

        await productCategoryRepository.AddAsync(productCategory);

        return await GetByIdAsync(productCategory.Id);
    }

    public async Task<ProductCategoryResponse> UpdateAsync(int id, UpdateProductCategoryRequest request)
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
