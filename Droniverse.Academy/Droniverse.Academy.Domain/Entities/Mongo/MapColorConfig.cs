
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;
public class MapColorConfig
{
    [BsonElement("ground")]
    public string Ground { get; set; } // Màu mặt đất (NOT NULL)

    [BsonElement("grid")]
    public string Grid { get; set; } // Màu lưới (NOT NULL)

    [BsonElement("border")]
    public string Border { get; set; } // Màu viền (NOT NULL)

    [BsonElement("ambient")]
    public string Ambient { get; set; } // Màu ánh sáng môi trường (NOT NULL)
}
