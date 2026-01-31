
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;
public class DroneColorConfig
{
    [BsonElement("fuselage")]
    public string Fuselage { get; set; } // Màu thân (NOT NULL)

    [BsonElement("fuselageEmi")]
    [BsonIgnoreIfNull]
    public string? FuselageEmi { get; set; } // Màu phát sáng thân (OPTIONAL)
                                             // Lưu ý: Dựa trên pattern nose/noseEmi, tôi đặt tên field này là fuselageEmi

    [BsonElement("nose")]
    public string Nose { get; set; } // Màu mũi (NOT NULL)

    [BsonElement("noseEmi")]
    [BsonIgnoreIfNull]
    public string? NoseEmi { get; set; } // Màu phát sáng mũi (OPTIONAL)

    [BsonElement("canopy")]
    public string Canopy { get; set; } // Màu vòm (NOT NULL)

    [BsonElement("wings")]
    public string Wings { get; set; } // Màu cánh (NOT NULL)

    [BsonElement("rotor")]
    public string Rotor { get; set; } // Màu cánh quạt (NOT NULL)

    [BsonElement("rotorEmi")]
    [BsonIgnoreIfNull]
    public string? RotorEmi { get; set; } // Màu phát sáng cánh quạt (OPTIONAL)
}
