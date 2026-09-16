using Droniverse.Shared.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Shared.JsonConverters;

public class CodeStatusEnumJsonConverter : JsonConverter<CodeStatusEnum>
{
    public override CodeStatusEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int intValue))
                {
                    return (CodeStatusEnum)intValue;
                }
                throw new JsonException($"Unable to convert \"{reader.GetDouble()}\" to enum type \"CodeStatusEnum\".");

            case JsonTokenType.String:
                string? stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    throw new JsonException("Empty string cannot be converted to enum type \"CodeStatusEnum\".");

                // Try direct enum name match (case-insensitive)
                if (Enum.TryParse<CodeStatusEnum>(stringValue, ignoreCase: true, out var result))
                    return result;

                // Try to parse as uppercase
                string upperValue = stringValue.ToUpperInvariant();
                return upperValue switch
                {
                    "ACTIVE" or "1" => CodeStatusEnum.Active,
                    "USED" or "2" => CodeStatusEnum.Used,
                    "EXPIRED" or "3" => CodeStatusEnum.Expired,
                    "DISABLED" or "4" => CodeStatusEnum.Disabled,
                    _ => throw new JsonException($"Unable to convert value \"{stringValue}\" to enum type \"CodeStatusEnum\". Valid values: Active (1), Used (2), Expired (3), Disabled (4).")
                };

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} when parsing enum \"CodeStatusEnum\".");
        }
    }

    public override void Write(Utf8JsonWriter writer, CodeStatusEnum value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}
