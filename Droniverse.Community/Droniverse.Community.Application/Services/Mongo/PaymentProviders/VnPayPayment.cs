using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Droniverse.Community.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Droniverse.Community.Application.Services.Mongo.PaymentProviders;

internal class VnPayPayment : IPayment
{

    private readonly ILogger<VnPayPayment> _logger;

    public PaymentMethod PMethod => PaymentMethod.VNPAY;
    public VnPayPayment(ILogger<VnPayPayment> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentResponse> CreatePaymentLinkAsync(Guid orderId, PaymentCreateDto dto, IConfiguration configuration)
    {
        var vnpUrl = configuration["VNPAY_URL"];
        var vnpHashSecret = configuration["VNPAY_HASH_SECRET"];
        var vnpParams = new SortedList<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Amount", ((long)dto.TotalAmount * 100).ToString() },
            { "vnp_TxnRef", orderId.ToString() }
        };

        string vnpSecureHash = ComputeVnPayHash(vnpParams, vnpHashSecret);
        string checkoutUrl = $"{vnpUrl}?{vnpParams.ToQueryString()}&vnp_SecureHash={vnpSecureHash}";

        return new PaymentResponse(checkoutUrl, orderId.ToString());
    }

    private string ComputeVnPayHash(SortedList<string, string> vnpParams, string secretKey)
    {
        var data = new StringBuilder();
        foreach (var kv in vnpParams)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        // Cắt bỏ dấu & cuối cù ng
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
        byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.ToString()));

        return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
    }
}

