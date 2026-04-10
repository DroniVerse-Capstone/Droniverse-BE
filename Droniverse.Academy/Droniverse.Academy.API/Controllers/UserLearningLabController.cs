using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/enrollments/{enrollmentId:guid}/labs")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserLearningLabController : ControllerBase
{
    private readonly ILogger<UserLearningLabController> _logger;
    private readonly ILabLearningService _service;

    public UserLearningLabController(ILogger<UserLearningLabController> logger, ILabLearningService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost("{labId:guid}/submit")]
    public async Task<IActionResult> SubmitLab(Guid enrollmentId, Guid labId, [FromBody] SubmitLabRequestDTO request)
    {
        try
        {
            var result = await _service.SubmitLabAsync(enrollmentId, labId, request);
            return Ok(SuccessResponse<SubmitLabResultDTO>.Create(result, "Nộp lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nộp lab thất bại.");
            throw;
        }
    }
}
