using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateLab([FromBody] CreateLabRequestDTO request)
    {
        try
        {
            var created = await _labService.CreateLabAsync(request);
            return StatusCode(201, SuccessResponse<LabClientViewDTO>.Create(created, "Tạo lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo lab thất bại.");
            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetLabs()
    {
        try
        {
            var labs = await _labService.GetLabsAsync();
            return Ok(SuccessResponse<IEnumerable<LabClientViewDTO>>.Create(labs, "Lấy danh sách lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách lab thất bại.");
            throw;
        }
    }

    [HttpGet("{labId:guid}")]
    [Authorize(Roles = $"{Roles.AdminOrSystemManager},{Roles.ClubMember}")]
    public async Task<IActionResult> GetLabById(Guid labId)
    {
        try
        {
            var lab = await _labService.GetLabByIdAsync(labId);
            return Ok(SuccessResponse<LabClientViewDTO>.Create(lab, "Lấy chi tiết lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết lab thất bại.");
            throw;
        }
    }

    [HttpPut("{labId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> UpdateLab(Guid labId, [FromBody] UpdateLabRequestDTO request)
    {
        try
        {
            var updated = await _labService.UpdateLabAsync(labId, request);
            return Ok(SuccessResponse<LabClientViewDTO>.Create(updated, "Cập nhật lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật lab thất bại.");
            throw;
        }
    }

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
