using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class ProductService(
    IProductRepository productRepository,
    IProductCategoryRepository productCategoryRepository,
    ICurrentUserProvider userProvider)
{
    private int CurrentSellerId => userProvider.SellerId ?? throw new UnauthorizedException("User is not a seller.");

    public async Task<PagedResult<ProductResponse>> SearchAsync(SearchProductsQuery query)
    {
        return await productRepository.SearchAsync(query);
    }

    public async Task<ProductResponse> GetByIdAsync(int id)
    {
        return await productRepository.GetDetailByIdAsync(id) ?? throw new NotFoundException("Product not found.");
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            SellerId = CurrentSellerId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockBalance = request.StockBalance,
            CreatedAt = DateTime.UtcNow
        };

        await productRepository.AddAsync(product);

        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await productRepository.GetByIdForSellerAsync(id, CurrentSellerId)
            ?? throw new NotFoundException("Product not found.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockBalance = request.StockBalance;

        await productRepository.UpdateAsync(product);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await productRepository.GetByIdForSellerAsync(id, CurrentSellerId)
            ?? throw new NotFoundException("Product not found.");

        if (await productRepository.HasOrdersAsync(id))
            throw new ConflictException("Product cannot be deleted because it belongs to existing orders.");

        await productRepository.DeleteAsync(product);
    }

    public async Task AddCategoryAsync(int productId, int productCategoryId)
    {
        var product = await productRepository.GetByIdForSellerAsync(productId, CurrentSellerId)
            ?? throw new NotFoundException("Product not found.");

        var productCategory = await productCategoryRepository.GetByIdAsync(productCategoryId)
            ?? throw new NotFoundException("Product category not found.");

        product.Categories.Add(productCategory);

        await productRepository.UpdateAsync(product);
    }

    public async Task RemoveCategoryAsync(int productId, int productCategoryId)
    {
        var product = await productRepository.GetByIdForSellerAsync(productId, CurrentSellerId)
            ?? throw new NotFoundException("Product not found.");

        var productCategory = await productCategoryRepository.GetByIdAsync(productCategoryId)
            ?? throw new NotFoundException("Product category not found.");

        if (!product.Categories.Any(c => c.Id == productCategoryId))
            throw new NotFoundException("Product category is not associated with the product.");

        product.Categories.Remove(productCategory);

        await productRepository.UpdateAsync(product);
    }
}
