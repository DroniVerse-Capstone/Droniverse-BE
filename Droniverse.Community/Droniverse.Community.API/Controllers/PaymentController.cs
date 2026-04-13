using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Text.Json;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentService _paymentService;
        private readonly ICurrentUserService _currentUserService;
        public PaymentController(
            ILogger<PaymentController> logger, 
            IPaymentService paymentService,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _paymentService = paymentService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Tạo liên kết thanh toán cho đơn hàng dựa trên orderId và thông tin thanh toán. #3. Luồng thanh toán
        /// <param name="orderId"></param>
        /// <param name="paymentCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResponse> CreatePaymentLinkAsync(Guid orderId, [FromBody] PaymentCreateDto paymentCreateDto)
        {

            var paymentResponse = await _paymentService.CreatePaymentLink(orderId, paymentCreateDto);
            return SuccessResponse<PaymentResponseDto>.Create(paymentResponse);

        }

        /// <summary>
        /// Kiểm tra trạng thái của order và payment. #2. #5. Luồng thanh toán
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("{orderId}")]
        public async Task<ApiResponse> GetPaymentStatusAsync(Guid orderId)
        {
            var paymentStatus = await _paymentService.GetPaymentStatus(orderId);
            return SuccessResponse<PaymentResponseDto>.Create(paymentStatus);
        }

        /// <summary>
        /// PayOS sẽ gửi webhook về endpoint này khi có sự kiện liên quan đến thanh toán (thành công, thất bại, hủy, v.v.). #4. Luồng thanh toán
        /// </summary>
        /// <param name="webhook"></param>
        /// <returns></returns>
        [HttpPost("webhook")]
        [HttpPut("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhookAsync()
        {
            try
            {
                // Request body buffering is already enabled in Program.cs middleware
                
                // Seek to beginning to ensure we read from start
                HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                
                // Read raw request body
                using var reader = new StreamReader(HttpContext.Request.Body, leaveOpen: true);
                string rawBody = await reader.ReadToEndAsync();
                
                _logger.LogInformation("Received webhook: {RawBody}", rawBody);
                
                // Check if body is empty
                if (string.IsNullOrWhiteSpace(rawBody))
                {
                    _logger.LogError("Webhook body is empty");
                    return BadRequest("Webhook body is empty");
                }

                // Reset body position for any downstream processing
                HttpContext.Request.Body.Position = 0;

                // Deserialize webhook
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var webhook = JsonSerializer.Deserialize<PayOSWebhookDto>(rawBody, options);

                if (webhook == null)
                {
                    _logger.LogError("Failed to deserialize webhook");
                    return BadRequest("Invalid webhook format");
                }

                if (webhook.Data == null)
                {
                    _logger.LogError("Webhook data is null");
                    return BadRequest("Webhook data is missing");
                }

                // Verify signature using data object (serialize with camelCase for proper signature computation)
                if (!await _paymentService.VerifyWebhookSignature(webhook.Data, webhook.Signature))
                {
                    _logger.LogError("Webhook signature invalid!");
                    return Unauthorized();
                }

                bool result = await _paymentService.HandleWebhook(webhook);
                _logger.LogInformation("Handle webhook result: {Result}", result);
                return result ? Ok() : BadRequest("Failed to process webhook");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, "Internal server error");
            }
        }
        

        /// <summary>
        /// Lấy danh sách các giao dịch thanh toán của người dùng hiện tại. 6#. Luồng thanh toán
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        public async Task<ApiResponse> GetMyPaymentsAsync()
        {
            // Lấy userId từ token hoặc context
            Guid userId = _currentUserService.UserId;
            string userName = _currentUserService.UserName;
            var payments = await _paymentService.GetPaymentsByUserId(userId);
            return SuccessResponse<IEnumerable<PaymentResponseDto>>.Create(
                payments,
                $"Lấy danh sách các giao dịch thanh toán của {userName} thành công");

        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost("cancel/{orderId}")]
        public async Task<ApiResponse> CancelPaymentAsync(Guid orderId)
        {
            bool result = await _paymentService.CancelPayment(orderId);
            if (result)
            {
                return SuccessResponse<string>.Create("Payment cancelled successfully.");
            }
            else
            {
                return SuccessResponse<string>.Create("Failed to cancel payment.");
            }
        }

        [HttpGet("payment-success")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess()
        {
        return Ok(new { message = "Payment successful" });
        }

        [HttpGet("payment-cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCancel()
        {
        return Ok(new { message = "Payment cancelled" });
        }
    }
}
