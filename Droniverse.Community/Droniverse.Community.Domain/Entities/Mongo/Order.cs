using Droniverse.Community.Domain.Enums;
namespace Droniverse.Community.Domain.Entities.Mongo;
public class Order // Document trong NoSql ~~ Table trong SequenceSql
{
    public Guid _id { get; set; }
    public Guid UserID { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } // PENDING, SUCCESS, FAILED
    public DateTime CreateAt { get; set; }

    public Guid InvoiceID { get; set; }

    public List<OrderItem> Items { get; set; }
    
    public Payment Payment { get; set; }

}