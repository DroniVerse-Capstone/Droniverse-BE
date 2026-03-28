using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Request.Mongo;

public record OrderCreateDto(
    decimal TotalAmount,
    PaymentMethod PaymentMethod,
    OrderItemDto Item
    )
{ }