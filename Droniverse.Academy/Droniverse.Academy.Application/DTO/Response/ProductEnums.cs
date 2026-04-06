using System.Text.Json.Serialization;

namespace Droniverse.Academy.Application.DTO.Response;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CurrencyType
{
    Unknown = 0,
    VND = 1,
    USD = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductStatus
{
    Unknown = 0,
    Draft = 1,
    Active = 2,
    Inactive = 3,
    Archived = 4
}
