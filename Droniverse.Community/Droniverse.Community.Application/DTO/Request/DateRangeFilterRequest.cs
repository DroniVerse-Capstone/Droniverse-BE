namespace Droniverse.Community.Application.DTO.Request
{
    /// <summary>
    /// Request DTO cho date range filter trong dashboard growth APIs.
    /// </summary>
    public class DateRangeFilterRequest
    {
        /// <summary>
        /// Ngày bắt đầu (inclusive). Format: yyyy-MM-dd hoặc ISO 8601 datetime.
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Ngày kết thúc (inclusive). Format: yyyy-MM-dd hoặc ISO 8601 datetime.
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
}
