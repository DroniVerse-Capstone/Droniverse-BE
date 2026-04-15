using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;

namespace Droniverse.Community.Application.IService.Mongo;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePaymentLink(Guid orderId, PaymentCreateDto paymenntCreateDto);
    Task<PaymentResponseDto> GetPaymentStatus(Guid orderId);
    Task<bool> VerifyWebhookSignature(PayOSWebhookData webhookData, string signature);
    Task<bool> HandleWebhook(PayOSWebhookDto webhook);
    Task<IEnumerable<PaymentResponseDto>> GetPaymentsByUserId(Guid userId);
    Task<bool> CancelPayment(Guid orderId);
}

