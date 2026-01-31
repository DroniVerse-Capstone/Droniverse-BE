
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;

public class Obstacle
{
    [BsonElement("id")]
    public string Id { get; set; } // Id riêng của obstacle
    [BsonElement("type")]
    public string Type { get; set; } // Loại obstacle, vd: "Cylinder", "Cube"
    [BsonElement("position")]
    public double[] Position { get; set; } // [x, y, z]
    [BsonElement("rotation")]
    public double[] Rotation { get; set; } // [x, y, z]

    [BsonElement("size")]
    public double[] Size { get; set; } // [x, y, z] - Dùng cho hình hộp

    // Dùng Nullable (double?) vì không phải obstacle nào cũng có Radius/Height
    [BsonElement("radius")]
    [BsonIgnoreIfNull]
    public double? Radius { get; set; }

    [BsonElement("height")]
    [BsonIgnoreIfNull]
    public double? Height { get; set; }

    [BsonElement("center")]
    public double[] Center { get; set; } // [x, y, z]
}
