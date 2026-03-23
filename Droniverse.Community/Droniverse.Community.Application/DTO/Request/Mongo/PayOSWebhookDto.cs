namespace Droniverse.Community.Application.DTO.Request.Mongo;

public record PayOSWebhookDto(
    string Code,
    string Desc,
    bool IsSuccess,
    PayOSWebhookData? Data,
    string Signature
    )
{ }