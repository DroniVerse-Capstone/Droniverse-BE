using Droniverse.Community.Domain.Enums;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record OrderItemDto(
    Guid ProductID,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    ProductType Type,
    int Quantity)
{
    public OrderItemDto() : this(Guid.Empty, ProductType.COURSE, 0)
    {
    }
}