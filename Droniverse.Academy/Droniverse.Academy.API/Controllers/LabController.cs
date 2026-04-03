using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/labs")]
[ApiController]
public class LabController : ControllerBase
{
    private readonly ILogger<LabController> _logger;
    private readonly ILabService _labService;

    public LabController(ILogger<LabController> logger, ILabService labService)
    {
        _logger = logger;
        _labService = labService;
    }

    /// <summary>
    /// Tạo mới bài lab vào kho lab (không tạo lesson tự động).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.AllRoles)]
    [SwaggerRequestExample(typeof(CreateLabRequestDTO), typeof(CreateLabRequestExample))]
    public async Task<IActionResult> CreateLab([FromBody] CreateLabRequestDTO request)
    {
        try
        {
            var created = await _labService.CreateLabAsync(request);
            return StatusCode(201, SuccessResponse<LabDetailResponseDTO>.Create(created, "Tạo lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo lab thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Tạo lesson từ lab có sẵn trong kho và gán lab vào lesson đó.
    /// </summary>
    [HttpPost("{labId:guid}/lessons")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateLessonFromLab(Guid labId, [FromBody] CreateLabLessonRequestDTO request)
    {
        try
        {
            var created = await _labService.CreateLessonFromLabAsync(labId, request);
            return StatusCode(201, SuccessResponse<LessonClientViewDTO>.Create(created, "Tạo lesson từ lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo lesson từ lab thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách bài lab.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.AllRoles)]
    public async Task<IActionResult> GetLabs([FromQuery] GetLabsQueryDTO query)
    {
        try
        {
            var labs = await _labService.GetLabsAsync(query);
            return Ok(SuccessResponse<object>.Create(labs, "Lấy danh sách lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách lab thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết bài lab.
    /// </summary>
    [HttpGet("{labId:guid}")]
    [Authorize(Roles = $"{Roles.AdminOrSystemManager},{Roles.ClubMember}")]
    public async Task<IActionResult> GetLabById(Guid labId)
    {
        try
        {
            var lab = await _labService.GetLabByIdAsync(labId);
            return Ok(SuccessResponse<LabDetailResponseDTO>.Create(lab, "Lấy chi tiết lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết lab thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật bài lab.
    /// </summary>
    [HttpPut("{labId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(UpdateLabRequestDTO), typeof(UpdateLabRequestExample))]
    public async Task<IActionResult> UpdateLab(Guid labId, [FromBody] UpdateLabRequestDTO request)
    {
        try
        {
            var updated = await _labService.UpdateLabAsync(labId, request);
            return Ok(SuccessResponse<LabDetailResponseDTO>.Create(updated, "Cập nhật lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật lab thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật nội dung lab.
    /// </summary>
    [HttpPut("{labId:guid}/content")]
    [Authorize(Roles = Roles.AllRoles)]
    public async Task<IActionResult> UpdateLabContent(Guid labId, [FromBody] UpdateLabContentRequestDTO request)
    {
        try
        {
            var updated = await _labService.UpdateLabContentAsync(labId, request);
            return Ok(SuccessResponse<LabContentResponseDTO>.Create(updated, "Cập nhật lab content thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật lab content thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa bài lab.
    /// </summary>
    [HttpDelete("{labId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> DeleteLab(Guid labId)
    {
        try
        {
            await _labService.DeleteLabAsync(labId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa lab thất bại.");
            throw;
        }
    }
}
