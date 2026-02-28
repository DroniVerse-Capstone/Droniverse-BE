public class PaginationResult<T>
{
    public IReadOnlyList<T> Data { get; }
    public int TotalRecords { get; }
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalPages { get; }

    public PaginationResult(
        IReadOnlyList<T> data,
        int totalRecords,
        int pageIndex,
        int pageSize)
    {
        Data = data;
        TotalRecords = totalRecords;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    }
}