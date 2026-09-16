using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers
{
    [Route("identity/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IClock _clock;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthService authService,
            IConfiguration configuration,
            IClock clock)
        {
            _logger = logger;
            _authService = authService;
            _configuration = configuration;
            _clock = clock;
        }

        /// <summary>
        /// Đăng nhập bằng email và mật khẩu. Trả về Thông tin người dùng, AccessToken và RefreshToken.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A <see cref="AuthResponse"/> containing the access token, refresh token, and user information.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(SuccessResponse<AuthResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginEmailDto request)
        {
            AuthResponse? response = await _authService.AuthenticatedUser(request);
            SetTokenCookies(response.AccessToken, response.RefreshToken);

            _logger.LogInformation($"User login with email {request.Email} successfully.");
            return Ok(SuccessResponse<AuthResponse>.Create(response, "Đăng nhập thành công."));
        }

        /// <summary>
        /// Đăng ký tài khoản mới bằng email và mật khẩu. Sau khi đăng ký thành công, người dùng sẽ nhận được email xác thực để kích hoạt tài khoản. Endpoint này trả về thông tin người dùng cùng với access token và refresh token, nhưng tài khoản sẽ chưa được kích hoạt cho đến khi người dùng xác thực email.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A <see cref="AuthResponse"/> containing the access token, refresh token, and user information.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(SuccessResponse<AuthResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            AuthResponse? response = await _authService.RegisterUser(request);
            _logger.LogInformation($"User register with email {request.Email} successfully.");
            return Ok(SuccessResponse<AuthResponse>.Create(response, "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản."));
        }

        [ProducesResponseType(typeof(SuccessResponse<AuthResponse>), StatusCodes.Status200OK)]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
        {
            AuthResponse? response = await _authService.RefreshToken(request.AccessToken, request.RefreshToken);
            _logger.LogInformation($"Token refreshed successfully.");
            return Ok(SuccessResponse<AuthResponse>.Create(response, "Token được làm mới thành công."));
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {

            await _authService.Logout();
            _logger.LogInformation($"User logged out successfully.");
            return Ok(SuccessResponse<string>.Create(null, "Đăng xuất thành công."));
        }

        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(SuccessResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            UserResponse? response = await _authService.VerifyEmailAsync(request.Token);
            _logger.LogInformation($"Email verified successfully.");
            return Ok(SuccessResponse<UserResponse>.Create(response, "Xác thực email thành công. Bạn có thể đăng nhập ngay bây giờ."));
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(SuccessResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUser()
        {
            UserResponse? response = await _authService.GetCurrentUserInfo();
            _logger.LogInformation($"Get current user info successfully.");
            return Ok(SuccessResponse<UserResponse?>.Create(response, "Lấy thông tin người dùng hiện tại thành công."));
        }

        [Authorize]
        [HttpPut("me")]
        [ProducesResponseType(typeof(SuccessResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] ProfileUpdateDto request)
        {
            bool response = await _authService.UpdateProfileAsync(request);
            _logger.LogInformation($"Cập nhật thông tin người dùng thành công.");
            return Ok(SuccessResponse<bool>.Create(response, "Cập nhật thông tin người dùng thành công."));
        }

        /// <summary>
        /// Service-to-Service authentication endpoint
        /// Được dùng bởi các microservice khác để lấy token cho internal communication
        /// </summary>
        /// <remarks>
        /// Endpoint này không yêu cầu user JWT token (AllowAnonymous)
        /// nhưng phải provide chính xác ServiceId + ApiKey từ config
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("service-token")]
        [ProducesResponseType(typeof(SuccessResponse<ServiceTokenResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetServiceToken([FromBody] ServiceCredentialsRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request?.ServiceId) || string.IsNullOrWhiteSpace(request?.ApiKey))
                {
                    _logger.LogWarning("Yêu cầu service token thiếu ServiceId hoặc ApiKey");
                    return BadRequest(new { message = "ServiceId và ApiKey là bắt buộc" });
                }

                // Validate API Key
                var expectedApiKey = _configuration["SERVICE_API_KEY"];
                if (string.IsNullOrEmpty(expectedApiKey))
                {
                    _logger.LogError("SERVICE_API_KEY not configured in environment");
                    return StatusCode(500, new { message = "Service configuration error" });
                }

                if (request.ApiKey != expectedApiKey)
                {
                    _logger.LogWarning($"Invalid API key attempt from service: {request.ServiceId}");
                    return Unauthorized(new { message = "Invalid API key" });
                }

                // Validate Service ID (optional whitelist check)
                var allowedServices = _configuration["SERVICE_ALLOWED_IDS"]?.Split(",") ?? new[] { "community-service", "academy-service", "notification-service" };
                if (!allowedServices.Contains(request.ServiceId))
                {
                    _logger.LogWarning($"Unauthorized service token request from: {request.ServiceId}");
                    return Unauthorized(new { message = "Service not allowed" });
                }

                // Generate service token
                var token = _authService.GenerateServiceToken(request.ServiceId);
                var expiresAt = _clock.Now.AddHours(1);

                _logger.LogInformation($"Service token issued for: {request.ServiceId}");

                return Ok(new ServiceTokenResponse
                {
                    Token = token,
                    ExpiresIn = new DateTimeOffset(expiresAt).ToUnixTimeSeconds(),
                    TokenType = "Bearer"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating service token");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, //sau này deploy thì sửa là true
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(120)
            };

            Response.Cookies.Append("AccessToken", accessToken, cookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, //sau này deploy thì sửa là true
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("RefreshToken", refreshToken, refreshCookieOptions);
        }
    }
}
 