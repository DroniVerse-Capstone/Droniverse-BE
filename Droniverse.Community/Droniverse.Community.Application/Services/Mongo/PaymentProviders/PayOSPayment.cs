using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Droniverse.Community.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using System.Text.Json;

namespace Droniverse.Community.Application.Services.Mongo.PaymentProviders;

internal class PayOSPayment : IPayment
{

    private readonly PayOSClient _payOSClient;
    private readonly ILogger<PayOSPayment> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _checksumKey;

    public PaymentMethod PMethod => PaymentMethod.PAYOS;

    public PayOSPayment(ILogger<PayOSPayment> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        var clientId = _configuration["PAYOS_CLIENT_ID"];
        var apiKey = _configuration["PAYOS_API_KEY"];
        _checksumKey = _configuration["PAYOS_CHECKSUM_KEY"] ?? "";

        _logger.LogInformation("PayOS Config - ClientId: {ClientId}, ApiKey: {ApiKey}, ChecksumKey: {ChecksumKey}",
                string.IsNullOrEmpty(clientId) ? "MISSING" : "OK",
                string.IsNullOrEmpty(apiKey) ? "MISSING" : "OK",
                string.IsNullOrEmpty(_checksumKey) ? "MISSING" : "OK");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(_checksumKey))
        {
            _logger.LogError("PayOS configuration is incomplete. Please check the environment variables.");
            throw new InvalidOperationException("PayOS configuration is incomplete. Please check the environment variables.");
        }

        try
        {
            var options = new PayOSOptions
            {
                ClientId = clientId,
                ApiKey = apiKey,
                ChecksumKey = _checksumKey,
            };
            _payOSClient = new PayOSClient(options);
            _logger.LogInformation("PayOSClient initialized successfully");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize PayOSClient");
            throw;
        }


        _logger = logger;
    }

    public async Task<PaymentResponse> CreatePaymentLinkAsync(Guid orderId, PaymentCreateDto dto, IConfiguration configuration)
    {
        string? returnUrl = configuration["PAYOS_RETURN_URL"] ?? "trang thanh toán thành công";
        string? cancelUrl = configuration["PAYOS_CANCEL_URL"] ?? "trang thanh toán thất bại";

        long orderCode = DateTimeOffset.Now.ToUnixTimeSeconds();
        string fullDescription = $"Order {orderCode}";
        string description = fullDescription.Length > 25
            ? fullDescription.Substring(0, 25)
            : fullDescription;

        _logger.LogInformation(
            "Creating PayOS Payment Link - OrderId: {OrderId}, OrderCode: {OrderCode}, Amount: {Amount}",
            orderId, orderCode, dto.TotalAmount);

        var request = new CreatePaymentLinkRequest
        {
            OrderCode = (int)orderCode,
            Amount = (long)dto.TotalAmount,
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl,
            Description = description,
        };

        var response = await _payOSClient.PaymentRequests.CreateAsync(request);

        if (response is null || string.IsNullOrWhiteSpace(response.CheckoutUrl))
        {
            _logger.LogError("PayOS returned null response.");
            throw new Exception("PayOS không trả về kết quả.");
        }

        var responseJson = JsonSerializer.Serialize(response);
        _logger.LogInformation("PayOS CreatePaymentLink Response: {Response}", responseJson);

        return new PaymentResponse(response.CheckoutUrl, orderCode.ToString());
    }
}

