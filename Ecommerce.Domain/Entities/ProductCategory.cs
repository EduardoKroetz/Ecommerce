namespace Ecommerce.Domain.Entities;

public class ProductCategory : BaseEntity
{
    public const int NameMaxLength = 100;

    public required string Name { get; set; }
}
