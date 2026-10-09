namespace Ecommerce.Application.DTOs;

public class PagedResult<T>
{
    public required List<T> Data { get; set; }

    public required int TotalItems { get; set; }

    public required int NextPage { get; set; }
    public required int PreviousPage { get; set; }

    public required int PageSize { get; set; }
    public required int PageNumber { get; set; }
}
