using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/assignments")]
[ApiController]
public class AssignmentController : ControllerBase
{
    private readonly ILogger<AssignmentController> _logger;
    private readonly IAssignmentService _service;

    public AssignmentController(ILogger<AssignmentController> logger, IAssignmentService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequestDTO request)
    {
        try
        {
            var created = await _service.CreateAssignmentAsync(request);
            return StatusCode(201, SuccessResponse<AssignmentClientViewDTO>.Create(created, "Tạo assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo assignment thất bại.");
            throw;
        }
    }
}
