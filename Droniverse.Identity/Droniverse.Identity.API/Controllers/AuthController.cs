using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Shared.DTOs;
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
            AuthResponse? response =  await _authService.AuthenticatedUser(request.Email, request.Password);
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
    }
}
 