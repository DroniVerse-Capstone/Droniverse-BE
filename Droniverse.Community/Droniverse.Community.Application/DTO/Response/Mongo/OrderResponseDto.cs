using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record OrderResponseDto(Guid OrderID, decimal TotalAmount, OrderStatus Status, DateTime CreateAt, OrderItemDto Item)
{
    public OrderResponseDto() : this(Guid.Empty, 0, OrderStatus.PENDING, DateTime.MinValue, default)
    {
    }
}