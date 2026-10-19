using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Policies;

namespace Ecommerce.Application.DTOs;

public class PaginationQuery
{
    [Range(PaginationPolicy.MinPageSize, PaginationPolicy.MaxPageSize)]
    public int PageSize { get; set; } = PaginationPolicy.DefaultPageSize;

    [Range(PaginationPolicy.MinPageNumber, int.MaxValue)]
    public int PageNumber { get; set; } = PaginationPolicy.MinPageNumber;

    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;
}
