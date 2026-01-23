using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities.Mongo;
public class Invoice
{
    public Guid _id { get; set; }
    public Guid OrderID { get; set; }
    public CustomerInfo CustomerInfo { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string ContentVN { get; set; } = null!;
    public string ContentEN { get; set; } = null!;
    public DateTime IssueAt { get; set; }
}

