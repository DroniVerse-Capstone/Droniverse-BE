using Droniverse.Community.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Community.Domain.Entities.Mongo;
public class Payment
{
    public string TransactionID { get; set; }
    [BsonRepresentation(BsonType.String)]
    public PaymentMethod PaymentMethod { get; set; }
    [BsonRepresentation(BsonType.String)]
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime TransactionDate { get; set; }
}

