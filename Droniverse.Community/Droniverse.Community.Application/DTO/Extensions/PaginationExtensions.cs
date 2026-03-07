using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Extensions;

public static class PaginationExtensions
{
    public static PaginationResult<IEnumerable<T>> ToPaginationResult<T>(this IEnumerable<T> source, SearchRequest searchRequest, int defaultPageSize = 5)
    {
        var list = source?.ToList() ?? new List<T>();
        int totalRecords = list.Count;

        int currentPage = searchRequest.CurrentPage <= 0 ? 1 : searchRequest.CurrentPage;
        int pageSize = searchRequest.PageSize <= 0 ? defaultPageSize : searchRequest.PageSize;

        var pagedData = list
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginationResult<IEnumerable<T>>(pagedData, totalRecords, currentPage, pageSize);
    }
}
