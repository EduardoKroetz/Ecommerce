namespace Ecommerce.Application.Policies;

/// <summary>
/// Source of truth for pagination limits used by list endpoints.
/// </summary>
public static class PaginationPolicy
{
    public const int DefaultPageSize = 25;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 9999;
    public const int MinPageNumber = 1;
}
