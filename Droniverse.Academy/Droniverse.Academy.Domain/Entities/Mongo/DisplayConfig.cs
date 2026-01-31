
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Academy.Domain.Entities;
public class DisplayConfig
{
    [BsonElement("trailEnabled")]
    [BsonIgnoreIfNull]
    public bool? TrailEnabled { get; set; } // Bật/Tắt vệt đường bay (OPTIONAL)

    [BsonElement("trailColor")]
    [BsonIgnoreIfNull]
    public string? TrailColor { get; set; } // Màu vệt đường bay (OPTIONAL)

    [BsonElement("trailMaxLength")]
    [BsonIgnoreIfNull]
    public double? TrailMaxLength { get; set; } // Chiều dài tối đa (OPTIONAL) - Dùng double cho Number

    [BsonElement("smoothing")]
    [BsonIgnoreIfNull]
    public bool? Smoothing { get; set; } // Làm mịn (OPTIONAL)

    [BsonElement("fade")]
    [BsonIgnoreIfNull]
    public bool? Fade { get; set; } // Hiệu ứng mờ dần (OPTIONAL)

    [BsonElement("sampleDistance")]
    [BsonIgnoreIfNull]
    public double? SampleDistance { get; set; } // Khoảng cách lấy mẫu (OPTIONAL)

    [BsonElement("lineWidth")]
    [BsonIgnoreIfNull]
    public double? LineWidth { get; set; } // Độ dày nét vẽ (OPTIONAL)
}
