using System.Linq.Expressions;
using Ecommerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Extensions;

public static class IQueryableExtensions
{
    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, TDto>> selector,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PagedResult<TDto>(items, totalCount, pagination.PageSize, pagination.PageNumber);
    }
}
