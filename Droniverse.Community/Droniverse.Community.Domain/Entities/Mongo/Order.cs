using Droniverse.Community.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Droniverse.Community.Domain.Entities.Mongo;
public class Order // Document trong NoSql ~~ Table trong SequenceSql
{
    //[BsonId]
    //[BsonRepresentation(BsonType.String)]
    public Guid _id { get; set; }

    //[BsonRepresentation(BsonType.String)]
    public Guid UserID { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    [BsonRepresentation(BsonType.String)]
    public OrderType OrderType { get; set; }
    public required Guid ClubID { get; set; }
    public decimal TotalAmount { get; set; }
    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; } // PENDING, SUCCESS, FAILED
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreateAt { get; set; }
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime ReceivedAt { get; set; }
    public OrderItem Item { get; set; }
    public Payment Payment { get; set; }

}