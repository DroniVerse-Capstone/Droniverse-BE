using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Droniverse.Community.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;


namespace Droniverse.Community.Application.Services.Mongo.PaymentProviders;

internal class MomoPayment : IPayment
{
    private readonly ILogger<MomoPayment> _logger;
    private readonly IConfiguration _configuration;

    public PaymentMethod PMethod => PaymentMethod.MOMO;

    public MomoPayment(ILogger<MomoPayment> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<PaymentResponse> CreatePaymentLinkAsync(
        Guid orderId, PaymentCreateDto paymentCreateDto, IConfiguration configuration)
    {
        var momoEndpoint = configuration["MOMO_ENDPOINT"];
        var momoPartnerCode = configuration["MOMO_PARTNER_CODE"];
        var momoSecret = configuration["MOMO_SECRET_KEY"];

        string rawHash = $"accessKey={momoPartnerCode}&amount={paymentCreateDto.TotalAmount}&orderId={orderId}...";
        string momoSignature = PaymentUtils.ComputeHmacSha256(rawHash, momoSecret);

        var momoResponse = await CallMomoApiAsync(momoEndpoint, rawHash, momoSignature);

        return new PaymentResponse(momoResponse.PayUrl, momoResponse.RequestId);
    }

    private async Task<MomoCreatePaymentResponse> CallMomoApiAsync(string endpoint, string rawData, string signature)
    {
        using var httpClient = new HttpClient();

        var payload = new
        {
            partnerCode = _configuration["MOMO_PARTNER_CODE"],
            requestId = Guid.NewGuid().ToString(),
            amount = 100000, // Thay bằng dynamic value
            orderId = Guid.NewGuid().ToString(),
            orderInfo = "Thanh toan don hang",
            redirectUrl = _configuration["MOMO_RETURN_URL"] ?? "https://yourdomain.com/return",
            ipnUrl = _configuration["MOMO_NOTIFY_URL"] ?? "https://yourdomain.com/notify",
            requestType = "captureWallet",
            extraData = "",
            signature = signature,
            lang = "vi"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(endpoint, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Lỗi gọi MoMo API: {Response}", responseString);
            throw new Exception("Không thể kết nối tới cổng thanh toán MoMo.");
        }

        var result = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(
            responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return result ?? throw new Exception("Phản hồi từ MoMo không hợp lệ.");
    }
}


