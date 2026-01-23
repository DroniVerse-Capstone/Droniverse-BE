using Droniverse.Community.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Community.Domain.Entities.Mongo;
public class Payment
{
    public string TransactionID { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime TransactionDate { get; set; }
}

