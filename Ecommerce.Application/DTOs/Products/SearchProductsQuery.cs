using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.DTOs.Products;

public class SearchProductsQuery : PaginationQuery
{
    public string? Name { get; set; }
    public List<int>? Categories { get; set; }
    public decimal? StartPrice { get; set; }
    public decimal? EndPrice { get; set; }
    public string? Sort { get; set; }

    public const string SortByNameAsc = "name_asc";
    public const string SortByPriceAsc = "price_asc";
    public const string SortByPriceDesc = "price_desc";
    public const string SortByCreatedAtAsc = "created_at_asc";
}
