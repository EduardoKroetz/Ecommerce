namespace Ecommerce.DTOs.ProductsCategories;

public class GetProductCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public static GetProductCategory FromModel(Models.ProductCategory productCategory)
    {
        return new GetProductCategory
        {
            Id = productCategory.Id,
            Name = productCategory.Name
        };
    }
}
