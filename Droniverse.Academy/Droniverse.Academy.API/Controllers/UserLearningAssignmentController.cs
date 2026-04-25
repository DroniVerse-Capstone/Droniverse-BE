using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/enrollments/{enrollmentId:guid}/assignments")]
[ApiController]
[Authorize(Roles = Roles.ClubMember)]
public class UserLearningAssignmentController : ControllerBase
{
    private readonly ILogger<UserLearningAssignmentController> _logger;
    private readonly IUserAssignmentService _service;

    public UserLearningAssignmentController(ILogger<UserLearningAssignmentController> logger, IUserAssignmentService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost("{assignmentId:guid}/submit")]
    public async Task<IActionResult> SubmitAssignment(
        Guid enrollmentId,
        Guid assignmentId,
        [FromBody] SubmitUserAssignmentRequestDTO request)
    {
        try
        {
            var result = await _service.SubmitAssignmentAsync(enrollmentId, assignmentId, request);
            return Ok(SuccessResponse<UserAssignmentSubmitResponseDTO>.Create(result, "Nộp assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nộp assignment thất bại.");
            throw;
        }
    }
}
