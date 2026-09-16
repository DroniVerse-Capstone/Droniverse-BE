using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Request.Mongo;

public record PayOSWebhookDto(
    string Code,
    string Desc,
    [property: JsonPropertyName("success")]
    bool IsSuccess,
    PayOSWebhookData? Data,
    string Signature
    )
{ }