using Droniverse.Community.Domain.Enums;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record OrderItemDto(
    Guid ProductID, 
    string ProductName,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    ProductType Type, 
    double UnitOfPrice, 
    int Quantity, 
    double Total)
{
    public OrderItemDto() : this(Guid.Empty, string.Empty, ProductType.DRONE, 0, 0, 0)
    {
    }
}