using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
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
        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginEmailDto request)
        {
            AuthResponse? response = await _authService.AuthenticatedUser(request);
            SetTokenCookies(response.AccessToken, response.RefreshToken);

            _logger.LogInformation($"User login with email {request.Email} successfully.");
            return Ok(SuccessResponse<AuthResponse>.Create(response, "Login successfully."));
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            AuthResponse? response = await _authService.RegisterUser(request);
            _logger.LogInformation($"User register with email {request.Email} successfully.");
            return Ok(SuccessResponse<AuthResponse>.Create(response, "Register successfully."));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
        {
            AuthResponse? response = await _authService.RefreshToken(request.AccessToken, request.RefreshToken);
            _logger.LogInformation($"Token refreshed successfully.");
            return Ok(SuccessResponse<AuthResponse>.Create(response, "Token refreshed successfully."));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {

            await _authService.Logout();
            _logger.LogInformation($"User logged out successfully.");
            return Ok(SuccessResponse<string>.Create(null, "Logout successfully."));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            UserResponse? response = await _authService.VerifyEmailAsync(request.Token);
            _logger.LogInformation($"Email verified successfully.");
            return Ok(SuccessResponse<UserResponse>.Create(response, "Email verified successfully."));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            UserResponse? response = await _authService.GetCurrentUserInfo();
            _logger.LogInformation($"Get current user info successfully.");
            return Ok(SuccessResponse<UserResponse?>.Create(response, "Get current user info successfully."));
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] ProfileUpdateDto request)
        {
            UserResponse? response = await _authService.UpdateProfileAsync(request);
            _logger.LogInformation($"Update current user info successfully.");
            return Ok(SuccessResponse<UserResponse>.Create(response, "Update current user info successfully."));
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
 