namespace Droniverse.Community.Application.DTO.Response
{
    public class SystemTransactionLogEntry
    {
        public string OrderID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class SystemTransactionLogsResponse
    {
        public List<SystemTransactionLogEntry> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
