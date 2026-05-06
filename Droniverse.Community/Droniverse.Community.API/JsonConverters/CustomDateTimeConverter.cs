using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Community.API.JsonConverters;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Format DateTime as ISO 8601 without the 'Z' suffix
        // "O" format: 2026-04-21T23:08:19.9097160
        // Then remove the trailing 'Z' if present
        var formatted = value.ToString("O").TrimEnd('Z');
        writer.WriteStringValue(formatted);
    }
}
