using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Droniverse.Community.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Droniverse.Community.Application.Services.Mongo.PaymentProviders;

public class CashPayment : IPayment
{
    public PaymentMethod PMethod => PaymentMethod.CASH;

    public Task<PaymentResponse> CreatePaymentLinkAsync(Guid orderId, PaymentCreateDto dto, IConfiguration configuration)
    {
        return Task.FromResult(
            new PaymentResponse(
                string.Empty, Guid.NewGuid().ToString()));
    }
}
