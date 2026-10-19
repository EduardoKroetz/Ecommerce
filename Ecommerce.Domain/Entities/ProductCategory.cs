namespace Ecommerce.Domain.Entities;

public class ProductCategory
{
    public const int NameMaxLength = 100;

    public int Id { get; set; }
    public required string Name { get; set; }
}
