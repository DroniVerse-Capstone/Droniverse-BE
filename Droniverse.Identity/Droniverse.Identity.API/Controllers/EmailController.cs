using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers;

[Route("identity/emails")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly ILogger<EmailController> _logger;
    private readonly IEmailService _emailService;

    public EmailController(ILogger<EmailController> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> SendTestEmail([FromBody] SendEmailRequest request)
    {
        await _emailService.SendRegistrationEmailAsync(
            request.Email,
            "Tuyn1",
            DateTime.UtcNow.AddHours(7).ToString(),
            "abc");
        return Ok("Test email sent successfully.");
    }
}

