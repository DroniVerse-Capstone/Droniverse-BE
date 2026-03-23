using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/labs")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserLabController : ControllerBase
{
    private readonly ILogger<UserLabController> _logger;
    private readonly IUserLabService _service;

    public UserLabController(ILogger<UserLabController> logger, IUserLabService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserLab([FromBody] CreateUserLabRequestDTO request)
    {
        try
        {
            var created = await _service.CreateUserLabAsync(request);
            return StatusCode(201, SuccessResponse<object>.Create(created, "Tạo user lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo user lab thất bại.");
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyUserLabs(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserLabCompletionFilter completion = UserLabCompletionFilter.All)
    {
        try
        {
            var result = await _service.GetMyUserLabsAsync(pageIndex, pageSize, MapCompletionFilter(completion));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách user lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách user lab thất bại.");
            throw;
        }
    }

    [HttpGet("{userLabId:guid}")]
    public async Task<IActionResult> GetMyUserLabById(Guid userLabId)
    {
        try
        {
            var result = await _service.GetMyUserLabByIdAsync(userLabId);
            return Ok(SuccessResponse<object>.Create(result, "Lấy chi tiết user lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết user lab thất bại.");
            throw;
        }
    }

    [HttpPut("{userLabId:guid}")]
    public async Task<IActionResult> UpdateMyUserLab(Guid userLabId, [FromBody] UpdateUserLabRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateMyUserLabAsync(userLabId, request);
            return Ok(SuccessResponse<object>.Create(updated, "Cập nhật user lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật user lab thất bại.");
            throw;
        }
    }

    [HttpDelete("{userLabId:guid}")]
    public async Task<IActionResult> DeleteMyUserLab(Guid userLabId)
    {
        try
        {
            await _service.DeleteMyUserLabAsync(userLabId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa user lab thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa user lab thất bại.");
            throw;
        }
    }

    private static bool? MapCompletionFilter(UserLabCompletionFilter completion)
    {
        return completion switch
        {
            UserLabCompletionFilter.All => null,
            UserLabCompletionFilter.Incompleted => false,
            UserLabCompletionFilter.Completed => true,
            _ => null
        };
    }
}
