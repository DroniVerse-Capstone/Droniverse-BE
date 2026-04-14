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

    /// <summary>
    /// Lấy trạng thái học lab của người dùng theo lab.
    /// Trả về thông tin lab và dữ liệu user lab nếu đã có.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment của người học.</param>
    /// <param name="labId">Mã lab.</param>
    [HttpGet("{labId:guid}")]
    public async Task<IActionResult> GetLabLearningState(Guid enrollmentId, Guid labId)
    {
        try
        {
            var result = await _service.GetLabLearningStateAsync(enrollmentId, labId);
            return Ok(SuccessResponse<LabLearningStateDTO>.Create(result, "Lấy dữ liệu lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy dữ liệu lab thất bại.");
            throw;
        }
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
