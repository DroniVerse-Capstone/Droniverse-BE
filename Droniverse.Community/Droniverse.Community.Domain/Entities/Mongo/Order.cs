using Droniverse.Community.Domain.Enums;
using MailKit.Search;
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
    public OrderType OrderType { get; set; }
    public required Guid ClubID { get; set; }
    public decimal TotalAmount { get; set; }
    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; } // PENDING, SUCCESS, FAILED
    public DateTime CreateAt { get; set; }
    public OrderItem Item { get; set; }
    public Payment Payment { get; set; }

}