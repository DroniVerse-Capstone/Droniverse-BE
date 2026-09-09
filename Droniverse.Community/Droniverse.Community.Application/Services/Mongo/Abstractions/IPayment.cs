using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Droniverse.Community.Application.Services.Mongo.Abstractions;

public interface IPayment
{
    Task<PaymentResponse> CreatePaymentLinkAsync(Guid orderId, PaymentCreateDto dto, IConfiguration configuration);
}

public record PaymentResponse(string CheckoutUrl, string PaymentLinkId);

