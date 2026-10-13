namespace Ecommerce.Application.DTOs;

public class PagedResult<T>
{
    public PagedResult(List<T> data, int totalCount, int pageSize, int pageNumber)
    {
        Data = data;
        TotalCount = totalCount;
        PageSize = pageSize;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public List<T> Data { get; private set; }

    public int TotalCount { get; private set; }

    public int PageSize { get; private set; }
    public int PageNumber { get; private set; }

    public int TotalPages { get; private set; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
