using Droniverse.Shared.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Shared.DTOs.Request;

public class OrderSearchRequest : SearchRequest
{
    public Guid? ClubId { get; set; }
    public Guid? BuyerId { get; set; }
    public Guid? CourseId { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? ReceiveDate { get; set; }
    [BsonRepresentation(BsonType.String)]
    public OrderStatusEnum? Status { get; set; }
    [BsonRepresentation(BsonType.String)]
    public OrderTypeEnum Type { get; set; }

}

