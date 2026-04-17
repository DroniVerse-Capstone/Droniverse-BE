using Droniverse.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/imports")]
[ApiController]
public class ImportController : ControllerBase
{
    private readonly ILogger<ImportController> _logger;
    private readonly IWebHostEnvironment _env;

    public ImportController(ILogger<ImportController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Tải xuống file mẫu câu hỏi quiz từ wwwroot.
    /// </summary>
    [HttpGet("quizquestion-template")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public IActionResult DownloadQuizQuestionTemplate()
    {
        try
        {
            var fileName = "quizquestion_template.xlsx";
            var webRoot = _env.WebRootPath ?? string.Empty;
            var filePath = Path.Combine(webRoot, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Template not found: {FilePath}", filePath);
                return NotFound(new { message = "Template not found." });
            }

            var stream = System.IO.File.OpenRead(filePath);
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tải file mẫu thất bại.");
            throw;
        }
    }
}
