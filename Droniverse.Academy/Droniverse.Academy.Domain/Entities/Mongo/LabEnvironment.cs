
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;

public class LabEnvironment
{

    [BsonElement("start")]
    public double[] Start { get; set; }// [x, y, z]
    [BsonElement("goal")]
    public double[] Goal { get; set; } // [x, y, z]
    [BsonElement("checkpoints")]
    public List<double[]> Checkpoints { get; set; } // Mảng chứa các mảng tọa độ
    [BsonElement("obstacles")]
    public List<Obstacle> Obstacles { get; set; }

}
