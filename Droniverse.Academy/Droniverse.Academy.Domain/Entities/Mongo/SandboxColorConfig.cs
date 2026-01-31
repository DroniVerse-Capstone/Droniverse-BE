
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;
public class SandboxColorConfig
{
    [BsonElement("drone")]
    public DroneColorConfig Drone { get; set; } = new();

    [BsonElement("map")]
    public MapColorConfig Map { get; set; } = new();
}
