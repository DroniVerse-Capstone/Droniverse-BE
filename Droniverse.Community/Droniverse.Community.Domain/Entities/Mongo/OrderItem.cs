using Droniverse.Community.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Community.Domain.Entities.Mongo;
public class OrderItem
{
    public Guid ProductID { get; set; }

    public string ProductName { get; set; }

    public ProductType Type { get; set; } // CODE, DRONE
    public double UnitOfPrice { get; set; }
    public int Quantity { get; set; }

    public double Total { get; set; }
}

