namespace Ecommerce.DTOs.Products;

public class GetProductsQuery : PaginationRequest
{
    public string? Name { get; set; }
    public List<int> Categories { get; set; } = [];
    public decimal? StartPrice { get; set; }
    public decimal? EndPrice { get; set; }
    public string Sort { get; set; } = SORT_BY_CREATED_AT_DESC;

    public const string SORT_BY_NAME_ASC = "name_asc";
    public const string SORT_BY_PRICE_ASC = "price_asc";
    public const string SORT_BY_PRICE_DESC = "price_desc";
    public const string SORT_BY_CREATED_AT_ASC = "created_at_asc";
    public const string SORT_BY_CREATED_AT_DESC = "created_at_desc";
}
