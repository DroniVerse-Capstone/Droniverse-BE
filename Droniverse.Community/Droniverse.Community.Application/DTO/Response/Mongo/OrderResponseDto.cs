using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record OrderResponseDto(
    Guid OrderID,
    OrderType Type,
    decimal TotalAmount,
    OrderStatus Status,
    DateTime CreateAt,
    OrderItemDto Item,
    PaymentResponseDto? Payment
)
{
    public OrderResponseDto() : this(Guid.Empty, default, 0, OrderStatus.PENDING, DateTime.MinValue, default, default)
    {
    }
}