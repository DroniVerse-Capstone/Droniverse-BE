namespace Droniverse.Shared.DTOs.Response
{
    public class PaginationResult<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public T Data { get; set; }

        public PaginationResult(T data, int totalRecords, int pageIndex, int pageSize)
        {
            Data = data;
            TotalRecords = totalRecords;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }
    }
}
