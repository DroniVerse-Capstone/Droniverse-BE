using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using System.Security.Cryptography;
using System.Text;

namespace Droniverse.Community.Application.Services.Mongo;

internal class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly PayOSClient _payOSClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly string _checksumKey;
    public PaymentService(
        IConfiguration configuration,
        ILogger<PaymentService> logger,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;

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
    }

    public async Task<PaymentResponseDto> CreatePaymentLink(Guid orderId, PaymentCreateDto paymentCreateDto)
    {
        if (paymentCreateDto is null)
        {
            throw new ArgumentNullException(nameof(paymentCreateDto), "PaymentCreateDto không được null");
        }

        if (paymentCreateDto.TotalAmount <= 0)
        {
            throw new ArgumentException("TotalAmount phải lớn hơn 0", nameof(paymentCreateDto.TotalAmount));
        }

        if (!Guid.TryParse(_currentUserService.UserID, out var userId))
        {
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực");
        }

        //paymentId
        Guid transactionId = orderId;
        try
        {
            FilterDefinition<Order> orderFilter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o._id, orderId),
                Builders<Order>.Filter.Eq(o => o.UserID, userId)
            );

            Order order = await _orderRepository.GetOrderByCondition(orderFilter)
                ?? throw new NotFoundException($"Order {orderId} không tồn tại hoặc không thuộc user hiện tại.");

            //Nếu đã có link pending thì trả về link đó, không tạo mới
            if (order.Payment is not null
                && order.Payment.PaymentStatus == PaymentStatus.PENDING
                && !string.IsNullOrWhiteSpace(order.Payment.PaymentUrl))
            {
                return new PaymentResponseDto(order._id, order.Payment.TransactionID, order.Payment.PaymentUrl, order.Payment.PaymentMethod, order.Payment.PaymentStatus, order.Payment.TransactionDate);
            }

            string? returnUrl = _configuration["PAYOS_RETURN_URL"] ?? "trang thanh toán thành công";
            string? cancelUrl = _configuration["PAYOS_CANCEL_URL"] ?? "trang thanh toán thất bại";
            _logger.LogInformation("URLs - Return: {ReturnUrl}, Cancel: {CancelUrl}", returnUrl, cancelUrl);

            // Sử dụng Unix timestamp thay vì Guid bytes (match FStreak-BE)
            long orderCode = DateTimeOffset.Now.ToUnixTimeSeconds();
            string fullDescription = $"Order {orderCode}";
            // Cắt ngắn description tối đa 25 ký tự (requirement của PayOS)
            string description = fullDescription.Length > 25 
                ? fullDescription.Substring(0, 25) 
                : fullDescription;

            _logger.LogInformation(
            "Creating PayOS Payment Link - OrderId: {OrderId}, OrderCode: {OrderCode}, Amount: {Amount}, ReturnUrl: {ReturnUrl}",
            orderId, orderCode, paymentCreateDto.TotalAmount, returnUrl);


            CreatePaymentLinkRequest? request = new CreatePaymentLinkRequest
            {
                OrderCode = (int)orderCode,  // Cast to int (PayOS requirement)
                Amount = (long)paymentCreateDto.TotalAmount,
                CancelUrl = cancelUrl,
                ReturnUrl = returnUrl,
                Description = description, // Cắt ngắn tối đa 25 ký tự
            };

            _logger.LogInformation("PayOS Request - OrderCode: {OrderCode}, Amount: {Amount}",
             request.OrderCode, request.Amount);


            // Gọi PayOs Api
            CreatePaymentLinkResponse? response = await _payOSClient.PaymentRequests.CreateAsync(request);

            if (response is null || string.IsNullOrWhiteSpace(response.CheckoutUrl))
            {
                _logger.LogError("PayOs return null response.");
                throw new Exception("PayOs không trả về kết quả.");
            }

            //Tạo entity Payment
            Payment payment = new Payment
            {
                TransactionID = orderId,
                PaymentMethod = paymentCreateDto.PaymentMethod,
                PaymentStatus = PaymentStatus.PENDING,
                TransactionDate = DateTime.UtcNow.AddHours(7), // +7 để dùng múi giờ VN
                PaymentUrl = response.CheckoutUrl
            };

            //Thêm payment vào order
            await _orderRepository.AddPayment(orderId, payment);

            _logger.LogInformation("Created Payment entity: " +
                "TransactionID: {TransID}, " +
                "PaymentMethod: {PaymentMethod}, " +
                "PaymentStatus: {PaymentStatus}, TransactionDate: {TransactionDate}",
                payment.TransactionID, payment.PaymentMethod, payment.PaymentStatus, payment.TransactionDate);



            return new PaymentResponseDto
            {
                OrderId = orderId,
                PaymentUrl = response.CheckoutUrl,
                Status = PaymentStatus.PENDING
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for user {UserId}", userId);
            try
            {
                await _orderRepository.UpdatePaymentStatus(orderId, PaymentStatus.FAILED, transactionId);
            }
            catch (Exception innerEx)
            {
                _logger.LogWarning(innerEx, "Unable to mark payment FAILED for order {OrderId}", orderId);
            }
            throw;
        }
    }

    public async Task<PaymentResponseDto> GetPaymentStatus(Guid orderId)
    {
        try
        {
            var paymemt = await _orderRepository.GetPaymentByOrderID(orderId);
            if (paymemt is null)
            {
                throw new NotFoundException($"Không tìm thấy payment cho order {orderId}");
            }

            try
            {
                PaymentLink? paymentInfo = await _payOSClient.PaymentRequests.GetAsync(orderId.ToString());
                if (paymentInfo is not null)
                {
                    paymemt.PaymentStatus = (PaymentStatus)paymentInfo.Status;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lấy thông tin thanh toán Payos cho đơn hàng {OrderId}. Trả về trạng thái hiện tại trong database.", orderId);
            }
            return new PaymentResponseDto
            {
                OrderId = orderId,
                PaymentUrl = paymemt.PaymentUrl,
                Status = paymemt.PaymentStatus,
                PaymentMethod = paymemt.PaymentMethod,
                TransactionDate = paymemt.TransactionDate,
                TransactionId = paymemt.TransactionID
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi lấy thông tin thanh toán cho đơn hàng {orderId}", orderId);
            throw;
        }
    }

    public async Task<bool> VerifyWebhookSignature(string webhookData, string signature)
    {
        try
        {
            string? computedSignature = ComputeHmacSha256(webhookData, _checksumKey);
            bool isValid = computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation("Web hook signature verification : {Result}", isValid ? "Valid" : "Invalid");
            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    public async Task<bool> HandleWebhook(PayOSWebhookDto webhook)
    {
        try
        {
            if (webhook.Data is null)
                return false;

            _logger.LogInformation("Processing webhook for orderId: {OrderId}", webhook.Data.OrderId);

            Payment? payment = await _orderRepository.GetPaymentByOrderID(webhook.Data.OrderId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for orderId: {OrderId}", webhook.Data.OrderId);
            }

            _logger.LogInformation("Webhook code: {Code}, webhook is success: {IsSuccess}", webhook.Code, webhook.IsSuccess);

            if (webhook.Code == "00" && webhook.IsSuccess)
            {
                _logger.LogInformation("webhook.Data.Code là {result}", webhook.Data.Code);
                payment.PaymentStatus = PaymentStatus.SUCCESS;
                payment.TransactionDate = DateTime.UtcNow.AddHours(7);
                payment.Reference = webhook.Data.Reference;
                payment.PaymentLinkID = webhook.Data.PaymentLinkId;
                payment.WebhookReceivedAt = DateTime.UtcNow.AddHours(7);

                await _orderRepository.UpdatePayment(webhook.Data.OrderId, payment);
            }
            return true;
        }
        catch (Exception)
        {

            throw;
        }

    }

    /// <summary>
    /// Tính toán HMAC-SHA256 hash của chuỗi dữ liệu đầu vào sử dụng khóa bí mật đã cấu hình. Kết quả trả về là một chuỗi thập lục phân viết thường, có thể được so sánh với chữ ký được gửi trong webhook để xác minh tính hợp lệ của yêu cầu.
    /// </summary>
    /// <param name="data">The input string to be hashed.</param>
    /// <param name="key">The secret key used to compute the HMAC-SHA256 hash. Cannot be null or empty.</param>
    /// <returns>Chuỗi ký tự thập lục phân viết thường biểu diễn giá trị băm HMAC-SHA256.</returns>
    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByUserId(Guid userId)
    {
        try
        {
            FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(p => p.UserID, userId);
            var payments = await _orderRepository.GetPaymentsByCondition(filter);
            return payments.Select(p => new PaymentResponseDto
            {
                OrderId = p.TransactionID,
                TransactionId = p.TransactionID,
                PaymentUrl = p.PaymentUrl,
                Status = p.PaymentStatus,
                PaymentMethod = p.PaymentMethod,
                TransactionDate = p.TransactionDate,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi lấy danh sách thanh toán cho user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> CancelPayment(Guid orderId)
    {
        try
        {
            FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(p => p._id, orderId);
            Order? order = await _orderRepository.GetOrderByCondition(filter);
            order.Status = OrderStatus.CANCELED;
            order.Payment.PaymentStatus = PaymentStatus.FAILED;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hủy thanh toán cho order {OrderId}", orderId);
            return false;
        }


    }
}

