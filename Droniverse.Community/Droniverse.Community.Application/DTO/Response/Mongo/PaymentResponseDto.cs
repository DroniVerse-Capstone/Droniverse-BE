using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record PaymentResponseDto(Guid? OrderId, Guid TransactionId, string? PaymentUrl, PaymentMethod PaymentMethod,  PaymentStatus Status, DateTime TransactionDate)
{
    public PaymentResponseDto() : this(Guid.Empty, Guid.Empty, string.Empty, default, default, default)
    {
    }
}