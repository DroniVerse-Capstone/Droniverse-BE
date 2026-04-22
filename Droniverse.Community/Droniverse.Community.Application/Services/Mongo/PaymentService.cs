using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using Droniverse.Shared.Messages.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Droniverse.Shared.Helpers;
using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Droniverse.Shared.Enums;

namespace Droniverse.Community.Application.Services.Mongo;

internal class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly PayOSClient _payOSClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;
    private readonly IEmailService _emailService;
    private readonly IOrderNotificationPublisher _orderNotificationPublisher;
    private readonly string _checksumKey;
    public PaymentService(
        IConfiguration configuration,
        ILogger<PaymentService> logger,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOrderRepository orderRepository,
        IInvoiceRepository invoiceRepository,
        IdentityMicroserviceClient identityMicroserviceClient,
        IEmailService emailService,
        AcademyMicroserviceClient academyMicroserviceClient,
        IOrderNotificationPublisher orderNotificationPublisher)
    {
        _orderRepository = orderRepository;
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _identityMicroserviceClient = identityMicroserviceClient;
        _emailService = emailService;
        _academyMicroserviceClient = academyMicroserviceClient;
        _orderNotificationPublisher = orderNotificationPublisher;

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

        _invoiceRepository = invoiceRepository;
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

            // Log đầy đủ response
            var responseJson = JsonSerializer.Serialize(response);
            _logger.LogInformation("PayOS CreatePaymentLink Response: {Response}", responseJson);

            //Tạo entity Payment
            Payment payment = new Payment
            {
                TransactionID = orderId,
                PaymentMethod = paymentCreateDto.PaymentMethod,
                PaymentStatus = PaymentStatus.PENDING,
                TransactionDate = DateTime.UtcNow.AddHours(7), // +7 để dùng múi giờ VN
                PaymentUrl = response.CheckoutUrl,
                PaymentLinkID = orderCode.ToString()  // Dùng OrderCode để match webhook
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
                Status = PaymentStatus.PENDING,

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
                    await _orderRepository.UpdatePayment(orderId, paymemt);

                    _logger.LogInformation("Updated payment status from PayOS: {Status}", paymentInfo.Status);

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

    public async Task<bool> VerifyWebhookSignature(PayOSWebhookData webhookData, string signature)
    {
        try
        {
            // Serialize data object with camelCase property names in sorted key order
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            string jsonData = JsonSerializer.Serialize(webhookData, options);

            // Parse JSON and sort keys
            using JsonDocument doc = JsonDocument.Parse(jsonData);
            var sortedKeys = doc.RootElement.EnumerateObject()
                .Select(p => p.Name)
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToList();

            // Build signature string: key1=value1&key2=value2&...
            var signatureData = new StringBuilder();
            for (int i = 0; i < sortedKeys.Count; i++)
            {
                var property = doc.RootElement.GetProperty(sortedKeys[i]);
                var value = property.ValueKind switch
                {
                    JsonValueKind.String => property.GetString() ?? "",
                    JsonValueKind.Number => property.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "",
                    _ => ""
                };

                signatureData.Append(sortedKeys[i]);
                signatureData.Append('=');
                signatureData.Append(value);

                if (i < sortedKeys.Count - 1)
                {
                    signatureData.Append('&');
                }
            }

            _logger.LogInformation("Signature data: {SignatureData}", signatureData.ToString());

            // Compute HMAC-SHA256
            string? computedSignature = ComputeHmacSha256(signatureData.ToString(), _checksumKey);
            bool isValid = computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation("Web hook signature verification : {Result}, Computed: {Computed}, Provided: {Provided}",
                isValid ? "Valid" : "Invalid", computedSignature, signature);

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
            if (webhook?.Data is null)
                return false;

            _logger.LogInformation("Processing webhook for OrderCode: {OrderCode}", webhook.Data.OrderCode);

            // Find order by OrderCode (this matches the code created during payment)
            Order? order = await _orderRepository.GetOrderByOrderCode(webhook.Data.OrderCode);
            if (order == null)
            {
                _logger.LogWarning("Payment not found for OrderCode: {OrderCode}", webhook.Data.OrderCode);
                return false;
            }

            Payment? payment = order.Payment;
            if (payment == null)
            {
                _logger.LogWarning("Payment object is null for PaymentLinkId: {PaymentLinkId}", webhook.Data.PaymentLinkId);
                return false;
            }

            _logger.LogInformation("Webhook code: {Code}, webhook is success: {IsSuccess}", webhook.Code, webhook.IsSuccess);

            if (webhook.Code == "00" && webhook.IsSuccess)
            {
                _logger.LogInformation("webhook.Data.Code is {result}", webhook.Data.Code);
                payment.PaymentStatus = PaymentStatus.SUCCESS;
                payment.Reference = webhook.Data.Reference;
                payment.PaymentLinkID = webhook.Data.PaymentLinkId;
                payment.WebhookReceivedAt = DateTime.UtcNow;

                order.Status = OrderStatus.SUCCESS;

                // Use cached email and username from order (saved when order was created)
                // No need to call Identity service in webhook context where HTTP context is not available
                string userEmail = order.UserEmail ?? string.Empty;
                string userName = order.UserName ?? "User";

                _logger.LogInformation("Payment SUCCESS for orderId: {OrderId}, Email: {Email}", order._id, userEmail);

                //Add Invoice
                Invoice invoice = new Invoice();
                invoice._id = Guid.NewGuid();
                invoice.TotalAmount = order.TotalAmount;
                invoice.ContentVN =
                        $"Thanh toán {order.Item.ProductNameVN} " +
                        $"(SL: {order.Item.Quantity}) " +
                        $"với tổng tiền {order.TotalAmount:N0} VND";
                invoice.ContentEN =
                        $"Payment for {order.Item.ProductNameEN} " +
                        $"(Qty: {order.Item.Quantity}) " +
                        $"with total amount {order.TotalAmount:N0} VND";
                invoice.IssueAt = DateTime.UtcNow.AddHours(7);
                invoice.CustomerInfo = new CustomerInfo
                {
                    UserID = order.UserID,
                    FullName = userName,  // Use cached username from order
                    Email = userEmail ?? string.Empty,
                    TaxCode = null,
                };
                invoice.Item = new InvoiceItem
                {
                    ProductID = order.Item.ProductID,
                    ProductNameVN = order.Item.ProductNameVN,
                    ProductNameEN = order.Item.ProductNameEN,
                    Currency = "VND",
                    Quantity = order.Item.Quantity,
                    Total = order.TotalAmount,
                    UnitPrice = order.Item.UnitOfPrice
                };

                Invoice? responseInvoice = await _invoiceRepository.AddInvoice(invoice);

                // Send payment confirmation email to user
                if (!string.IsNullOrWhiteSpace(userEmail))
                {
                    try
                    {
                        await _emailService.SendOrderConfirmationEmailAsync(
                            email: userEmail,
                            userName: userName ?? "User",
                            orderId: order._id.ToString(),
                            orderDate: order.CreateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                            productId: order.Item.ProductID.ToString(),
                            productNameVN: order.Item.ProductNameVN,
                            productNameEN: order.Item.ProductNameEN,
                            type: order.OrderType.ToString(),
                            unitOfPrice: order.Item.UnitOfPrice,
                            quantity: order.Item.Quantity,
                            totalAmount: order.TotalAmount);
                        _logger.LogInformation("Payment confirmation email sent to {Email} for OrderId: {OrderId}", userEmail, order._id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send payment confirmation email to {Email} for OrderId: {OrderId}", userEmail, order._id);
                    }
                }
                else
                {
                    _logger.LogWarning("Cannot send payment confirmation email - user email not found for UserId: {UserId}", order.UserID);
                }

                _logger.LogInformation("Payment SUCCESS for orderId: {OrderId}", order._id);

                //Publish notification event after successful payment
                try
                {
                    if (!string.IsNullOrWhiteSpace(userEmail))
                    {
                        var notificationEvent = new PaymentSuccessfulNotificationMessage(
                            UserId: order.UserID,
                            OrderId: order._id,
                            UserEmail: userEmail,
                            UserName: userName ?? "User",
                            Amount: order.TotalAmount,
                            PaidAt: DateTime.UtcNow
                        );

                        await _orderNotificationPublisher.PublishPaymentSuccessfulAsync(notificationEvent);
                        _logger.LogInformation($"Payment successful notification published for order {order._id}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish payment successful notification");
                    // Don't throw - payment is already successful, notification failure shouldn't fail the flow
                }

                // ===== Handle ClubCourse and Generate Codes for successful payment =====
                try
                {
                    // Get product info to access ReferenceID (CourseID)
                    Product? product = await _unitOfWork.Products.GetByCondition(
                        p => p.ProductID == order.Item.ProductID && p.Status == ProductStatus.ACTIVE,
                        query => query.AsNoTracking());

                    if (product != null)
                    {
                        bool isMember = order.OrderType == OrderType.USER_PURCHASE;


                        // Tạo và gán mã cho người dùng
                        HttpClients.GenerateCodesRequestDTO codeRequest = new HttpClients.GenerateCodesRequestDTO
                        {
                            ClubId = order.ClubID,
                            CourseId = product.ReferenceID,
                            Quantity = order.Item.Quantity
                        };

                        CodeResponse codeResponse = await _academyMicroserviceClient.GenerateAssignCodesAsync(codeRequest, order.UserEmail);

                        if (codeResponse == null || codeResponse.CodeID == null)
                        {
                            throw new Exception("Gán mã cho thành viên clb thất bại. Vui lòng liên hệ hỗ trợ.");
                        }

                        _logger.LogInformation("Generated and assigned codes for member - OrderId: {OrderId}, CodeId: {CodeId}",
                            order._id, codeResponse.CodeID);

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing ClubCourse/Codes for successful payment - OrderId: {OrderId}", order._id);
                    // Don't throw here - payment is already successful, just log the error
                }

                // Club manager nhận hoa hồng 10%
                Club? club = await _unitOfWork.Clubs.GetByCondition(
                    c => c.ClubID == order.ClubID,
                    query => query.AsNoTracking());
                if (club != null)
                {
                    try
                    {
                        Guid managerId = club.ManagerID;
                        decimal commissionAmount = order.TotalAmount * 0.1m;
                        Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.OwnerID == managerId);
                        if (wallet == null)
                            throw new NotFoundException("Không tìm thấy ví cho managerId: " + managerId);

                        wallet.UpdateBalance(commissionAmount);
                        await _unitOfWork.Wallets.Update(wallet);

                        //tạo transaction
                        Transaction transaction = new Transaction(
                            walletId: wallet.WalletID,
                            amount: (int)commissionAmount,
                            type: TransactionType.COMMISSION,
                            referenceID: order._id);

                        await _unitOfWork.Transactions.Add(transaction);

                        await _unitOfWork.SaveChangeAsync();

                        _logger.LogInformation("Added commission {CommissionAmount} to manager {ManagerId} wallet",
                            commissionAmount, managerId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error adding commission to manager wallet - OrderId: {OrderId}", order._id);
                        // Có thể decide là throw hay log warning tùy business logic
                        throw;
                    }
                } else
                {
                    throw new NotFoundException($"Không tìm thấy club cho ClubID: {order.ClubID}");
                }
            }
            else if (webhook.Code == "05")
            {
                payment.PaymentStatus = PaymentStatus.CANCELLED;
                order.Status = OrderStatus.CANCELLED;
                _logger.LogWarning("Payment CANCELLED for orderId: {OrderId}", order._id);
            }
            else if (!webhook.IsSuccess)
            {
                payment.PaymentStatus = PaymentStatus.FAILED;
                order.Status = OrderStatus.FAILED;
                _logger.LogWarning("Payment FAILED for orderId: {OrderId}, Code: {Code}",
                    order._id, webhook.Code);
            }
            else
            {
                _logger.LogWarning("Unknown webhook code: {Code}", webhook.Code);
                return false;
            }
            payment.TransactionDate = DateTime.UtcNow.AddHours(7);
            payment.WebhookReceivedAt = DateTime.UtcNow.AddHours(7);

            await _orderRepository.UpdatePayment(order._id, payment);
            await _orderRepository.UpdateOrder(order);
            _logger.LogInformation("Updated payment status to {Status}", payment.PaymentStatus);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling webhook for PaymentLinkId: {PaymentLinkId}", webhook?.Data?.PaymentLinkId);
            return false;
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
            order.Status = OrderStatus.CANCELLED;
            order.Payment.PaymentStatus = PaymentStatus.FAILED;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hủy thanh toán cho order {OrderId}", orderId);
            return false;
        }
    }

    /// <summary>
    /// Get user info (email and name) from Identity service
    /// </summary>
    private async Task<(string? Email, string? UserName)> GetUserInfoAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Fetching user info from Identity service for UserId: {UserId}", userId);
            UserResponse user = await _identityMicroserviceClient.GetUserByUserID(userId);
            if (user != null)
            {
                _logger.LogInformation("Successfully retrieved user info for UserId: {UserId}", userId);
                return (user.Email, user.Username);
            }
            _logger.LogWarning("User not found for UserId: {UserId}", userId);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user info from Identity service for UserId: {UserId}", userId);
            return (null, null);
        }
    }

    /// <summary>
    /// Get user email from Identity service for webhook email sending
    /// </summary>
    private async Task<string?> GetUserEmailAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Fetching user email from Identity service for UserId: {UserId}", userId);
            var user = await _identityMicroserviceClient.GetUserByUserID(userId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogInformation("Successfully retrieved email for UserId: {UserId}", userId);
                return user.Email;
            }
            _logger.LogWarning("User not found or email is null for UserId: {UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user email from Identity service for UserId: {UserId}", userId);
            return null;
        }
    }
}

