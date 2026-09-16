
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;

public class LabContent
{
    public string _id { get; set; }  // lưu string
    public BsonValue Environment { get; set; } = new BsonDocument();
}