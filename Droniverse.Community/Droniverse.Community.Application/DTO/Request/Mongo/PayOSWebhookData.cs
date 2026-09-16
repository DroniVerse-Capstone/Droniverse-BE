using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Request.Mongo;

public record PayOSWebhookData(
    [property: JsonPropertyName("orderCode")]
    long OrderCode,
    decimal Amount,
    string Description,
    string AccountNumber,
    string Reference,
    string TransactionDateTime,
    string Currency,
    string PaymentLinkId,
    string Code,
    string Desc,
    string CounterAccountBankId,
    string CounterAccountBankName,
    string CounterAccountName,
    string CounterAccountNumber,
    string VirtualAccountName,
    string VirtualAccountNumber
    )
{ }