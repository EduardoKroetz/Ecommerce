using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class Product : BaseEntity
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    private readonly List<ProductCategory> _categories = [];

    private Product() { Name = null!; } // EF

    public Product(int sellerId, string name, string? description, decimal price, int stockBalance, DateTime createdAt)
    {
        SellerId = sellerId;
        CreatedAt = createdAt;
        Name = null!;

        if (stockBalance < 0)
            throw new DomainException(EDomainExceptionCode.ProductStockInvalid, "Stock balance must be a non-negative value.");

        StockBalance = stockBalance;

        Update(name, description, price);
    }

    public int SellerId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockBalance { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();

    // Stock is not part of Update: it only changes through atomic operations in the repository (domain-model.md)
    public void Update(string name, string? description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > NameMaxLength)
            throw new DomainException(EDomainExceptionCode.ProductNameInvalid, $"Product name is required and cannot exceed {NameMaxLength} characters.");

        if (description?.Length > DescriptionMaxLength)
            throw new DomainException(EDomainExceptionCode.ProductDescriptionTooLong, $"Product description cannot exceed {DescriptionMaxLength} characters.");

        if (price <= 0)
            throw new DomainException(EDomainExceptionCode.ProductPriceInvalid, "Product price must be a positive value.");

        Name = name;
        Description = description;
        Price = price;
    }

    public void AddCategory(ProductCategory category)
    {
        if (_categories.Any(c => c.Id == category.Id))
            return;

        _categories.Add(category);
    }

    public void RemoveCategory(int categoryId)
    {
        var category = _categories.FirstOrDefault(c => c.Id == categoryId)
            ?? throw new DomainException(EDomainExceptionCode.ProductCategoryNotAssociated, "Product category is not associated with the product.");

        _categories.Remove(category);
    }
}
