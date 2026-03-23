using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/modules")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserModuleController : ControllerBase
{
    private readonly ILogger<UserModuleController> _logger;
    private readonly IUserModuleService _service;

    public UserModuleController(ILogger<UserModuleController> logger, IUserModuleService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserModule([FromBody] CreateUserModuleRequestDTO request)
    {
        try
        {
            var created = await _service.CreateUserModuleAsync(request);
            return StatusCode(201, SuccessResponse<object>.Create(created, "Tạo user module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo user module thất bại.");
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyUserModules(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserModuleCompletionFilter completion = UserModuleCompletionFilter.All)
    {
        try
        {
            var result = await _service.GetMyUserModulesAsync(pageIndex, pageSize, MapCompletionFilter(completion));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách user module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách user module thất bại.");
            throw;
        }
    }

    [HttpGet("{moduleId:guid}")]
    public async Task<IActionResult> GetMyUserModule(Guid moduleId)
    {
        try
        {
            var result = await _service.GetMyUserModuleAsync(moduleId);
            return Ok(SuccessResponse<object>.Create(result, "Lấy chi tiết user module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết user module thất bại.");
            throw;
        }
    }

    [HttpPut("{moduleId:guid}")]
    public async Task<IActionResult> UpdateMyUserModule(Guid moduleId, [FromBody] UpdateUserModuleRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateMyUserModuleAsync(moduleId, request);
            return Ok(SuccessResponse<object>.Create(updated, "Cập nhật user module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật user module thất bại.");
            throw;
        }
    }

    [HttpDelete("{moduleId:guid}")]
    public async Task<IActionResult> DeleteMyUserModule(Guid moduleId)
    {
        try
        {
            await _service.DeleteMyUserModuleAsync(moduleId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa user module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa user module thất bại.");
            throw;
        }
    }

    private static bool? MapCompletionFilter(UserModuleCompletionFilter completion)
    {
        return completion switch
        {
            UserModuleCompletionFilter.All => null,
            UserModuleCompletionFilter.Incompleted => false,
            UserModuleCompletionFilter.Completed => true,
            _ => null
        };
    }
}
