using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record OrderResponseDto(
    Guid OrderID,
    OrderType Type,
    decimal TotalAmount,
    OrderStatus Status,
    DateTime CreateAt,
    OrderItemDto Item,
    PaymentResponseDto? Payment,
    UserResponse User
)
{
    public OrderResponseDto() : this(Guid.Empty, default, 0, OrderStatus.PENDING, DateTime.MinValue, default, default, default)
    {
    }
}