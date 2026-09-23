namespace Ecommerce.DTOs;

public class PaginationRequest
{
    public int PageSize { get; set; } = 25;
    public int PageNumber { get; set; } = 1;

    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;
}
