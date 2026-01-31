
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;
public class SimulateConfig
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userID")]
    public string UserId { get; set; } // Reference to UserInfo logic

    [BsonElement("sandboxColorConfig")]
    public SandboxColorConfig SandboxColorConfig { get; set; } = new();

    [BsonElement("displayConfig")]
    public DisplayConfig DisplayConfig { get; set; } = new();
}
