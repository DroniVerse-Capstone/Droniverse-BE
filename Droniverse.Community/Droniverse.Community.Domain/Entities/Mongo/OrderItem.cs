using Droniverse.Community.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Community.Domain.Entities.Mongo;
public class OrderItem
{
    public Guid ProductID { get; set; }
    public required string ProductNameVN { get; set; }
    public required string ProductNameEN { get; set; }
    [BsonRepresentation(BsonType.String)]
    public ProductType Type { get; set; } // CODE, DRONE
    public decimal UnitOfPrice { get; set; }
    public int Quantity { get; set; }

    public decimal Total { get; set; }
}

