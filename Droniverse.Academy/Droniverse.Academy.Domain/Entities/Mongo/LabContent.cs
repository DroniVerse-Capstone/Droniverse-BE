
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;

public class LabContent
{
    public string _id { get; set; }  // lưu string
    public BsonDocument Environment { get; set; }
}