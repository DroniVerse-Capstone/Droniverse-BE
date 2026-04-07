using System.Text.Json.Serialization;

namespace Droniverse.Academy.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CodeUsageStatus
{
    UNUSED,
    USED,
}
