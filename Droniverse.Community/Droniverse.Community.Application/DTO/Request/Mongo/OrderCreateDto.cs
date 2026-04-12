using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Domain.Enums;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Request.Mongo;

public record OrderCreateDto(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    PaymentMethod PaymentMethod,
    OrderItemDto Item
    )
{ }