using System.Text.Json.Serialization;

namespace demo_api.models.Models;

public class PagedList<T>
{
    public IEnumerable<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public int TotalPages => PageSize != 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    [JsonConstructor]
    public PagedList(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}