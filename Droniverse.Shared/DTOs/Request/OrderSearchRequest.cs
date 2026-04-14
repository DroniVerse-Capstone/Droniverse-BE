using Droniverse.Shared.Enums;

namespace Droniverse.Shared.DTOs.Request;

public class OrderSearchRequest : SearchRequest
{
    public Guid? ClubId { get; set; }
    public Guid? BuyerId { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public OrderStatusEnum Status { get; set; }
}

