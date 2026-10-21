using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<PagedResult<ProductResponse>> SearchAsync(SearchProductsQuery query);
    Task<ProductResponse?> GetDetailByIdAsync(int id);

    Task<Product?> GetByIdForSellerAsync(int id, int sellerId);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Product product);

    Task<bool> ExistsAsync(int id);
    Task<bool> HasOrdersAsync(int id);

    Task<bool> TryDecrementStockAsync(int productId, int quantity);
}
