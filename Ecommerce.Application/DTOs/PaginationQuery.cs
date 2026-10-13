using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs;

public class PaginationQuery
{
    [Range(1, 9999)]
    public int PageSize { get; set; } = 25;

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;
}
