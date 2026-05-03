using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> HandleWebhookAsync([FromBody] PayOSWebhookDto? webhook)
        {
            try
            {
                if (webhook == null)
                {
                    _logger.LogError("Webhook payload is null or could not be bound from request body");
                    return BadRequest("Invalid webhook payload");
                }

                _logger.LogInformation("Received webhook: {Webhook}", JsonSerializer.Serialize(webhook));

                if (webhook.Data == null)
                {
                    _logger.LogError("Webhook data is null");
                    return BadRequest("Webhook data is missing");
                }

                if (string.IsNullOrWhiteSpace(webhook.Signature))
                {
                    _logger.LogError("Webhook signature is missing");
                    return BadRequest("Webhook signature is missing");
                }

                // Verify signature using data object (serialize with camelCase for proper signature computation)
                if (!await _paymentService.VerifyWebhookSignature(webhook.Data, webhook.Signature))
                {
                    _logger.LogError("Webhook signature invalid!");
                    return Unauthorized();
                }

                // Process webhook but always return 200 OK to acknowledge receipt
                // PayOS requires 200 OK to confirm webhook was received
                bool result = await _paymentService.HandleWebhook(webhook);
                _logger.LogInformation("Handle webhook result: {Result}", result);
                


                // Business logic:

                // Always return 200 OK - PayOS just needs confirmation that endpoint received it
                // Even if order not found, we return OK (may be a test webhook or delayed delivery)
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                // Still return 200 OK on error to avoid webhook retry loop
                // Errors are logged and can be reviewed later
                return Ok();
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
