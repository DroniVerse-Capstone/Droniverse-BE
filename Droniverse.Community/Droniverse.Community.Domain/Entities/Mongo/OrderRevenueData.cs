namespace Droniverse.Community.Domain.Entities.Mongo;

public class OrderRevenueData
{
    public Guid ProductId { get; set; }
    public decimal Revenue { get; set; }
    public DateTime PaidAt { get; set; }
}
