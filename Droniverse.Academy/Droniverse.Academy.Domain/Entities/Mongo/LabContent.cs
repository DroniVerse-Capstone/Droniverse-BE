
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;

public class LabContent
{
    public Guid _id { get; set; } // LabID
    [BsonElement("environment")] // tên của property trong collection thực tế
    public LabEnvironment Environment { get; set; }
}