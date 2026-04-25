using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/manager/assignments/submissions")]
[ApiController]
[Authorize(Roles = Roles.ClubManager)]
public class ClubManagerAssignmentController : ControllerBase
{
    private readonly ILogger<ClubManagerAssignmentController> _logger;
    private readonly IUserAssignmentService _service;

    public ClubManagerAssignmentController(ILogger<ClubManagerAssignmentController> logger, IUserAssignmentService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost("{userAssignmentId:guid}/review")]
    public async Task<IActionResult> ReviewAssignment(Guid userAssignmentId, [FromBody] ReviewUserAssignmentRequestDTO request)
    {
        try
        {
            var result = await _service.ReviewAssignmentAsync(userAssignmentId, request);
            return Ok(SuccessResponse<UserAssignmentReviewResponseDTO>.Create(result, "Chấm assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chấm assignment thất bại.");
            throw;
        }
    }
}
