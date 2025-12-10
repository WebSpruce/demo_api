using demo_api.models.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Infrastructure;

internal static class Pagination
{
    internal static async Task<PagedList<T>> Paginate<T>(IQueryable<T> query, int? pageNumber, int? pageSize,
        CancellationToken token = default)
    {
        int currentPage = pageNumber.GetValueOrDefault(1);
        int currentPageSize = pageSize.GetValueOrDefault(10); // A sensible default, like 10

        if (currentPage <= 0)
            currentPage = 1;
        
        if (currentPageSize <= 0)
            currentPageSize = 10;
        
        int totalCount = await query.CountAsync(token);

        var items = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .ToListAsync(token);

        return new PagedList<T>(items, currentPage, currentPageSize, totalCount);
    }
}