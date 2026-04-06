public class PaginationResult<T>
{
    public T Data { get; }
    public int TotalRecords { get; }
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalPages { get; }

    public PaginationResult(
        T data,
        int totalRecords,
        int pageIndex,
        int pageSize)
    {
        Data = data;
        TotalRecords = totalRecords;
        PageIndex = pageIndex < 1 ? 1 : pageIndex;
        PageSize = pageSize < 1 ? 5 : pageSize;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    }
}