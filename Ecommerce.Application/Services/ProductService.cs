using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Extensions;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class ProductService(
    IProductRepository productRepository,
    IProductCategoryRepository productCategoryRepository,
    ICurrentUserProvider userProvider,
    TimeProvider timeProvider)
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
        var product = new Product(
            CurrentSellerId,
            request.Name,
            request.Description,
            request.Price,
            request.StockBalance,
            timeProvider.GetUtcDateTime());

        await productRepository.AddAsync(product);

        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await productRepository.GetByIdForSellerAsync(id, CurrentSellerId)
            ?? throw new NotFoundException("Product not found.");

        product.Update(request.Name, request.Description, request.Price);

        await productRepository.UpdateAsync(product);

        return await GetByIdAsync(id);
    }

    // Stock changes only through the atomic repository operation, never by overwriting the entity (domain-model.md)
    public async Task<ProductResponse> AdjustStockAsync(int id, AdjustProductStockRequest request)
    {
        if (!await productRepository.ExistsForSellerAsync(id, CurrentSellerId))
            throw new NotFoundException("Product not found.");

        if (!await productRepository.TryAdjustStockAsync(id, request.Quantity))
            throw new DomainException(EDomainExceptionCode.InsufficientStock, "Stock balance cannot become negative.");

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

        product.AddCategory(productCategory);

        await productRepository.UpdateAsync(product);
    }

    public async Task RemoveCategoryAsync(int productId, int productCategoryId)
    {
        var product = await productRepository.GetByIdForSellerAsync(productId, CurrentSellerId)
            ?? throw new NotFoundException("Product not found.");

        var productCategory = await productCategoryRepository.GetByIdAsync(productCategoryId)
            ?? throw new NotFoundException("Product category not found.");

        product.RemoveCategory(productCategory.Id);

        await productRepository.UpdateAsync(product);
    }
}
