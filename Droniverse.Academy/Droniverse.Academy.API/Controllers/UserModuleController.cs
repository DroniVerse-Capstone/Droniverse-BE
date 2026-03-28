using Droniverse.Academy.API.Enums;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

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

    /// <summary>
    /// Tạo user module cho người dùng hiện tại.
    /// </summary>
    /// <param name="request">Thông tin user module cần tạo.</param>
    [HttpPost]
    [SwaggerRequestExample(typeof(CreateUserModuleRequestDTO), typeof(CreateUserModuleRequestExample))]
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

    /// <summary>
    /// Lấy danh sách user module của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="completion">Bộ lọc trạng thái hoàn thành.</param>
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

    /// <summary>
    /// Lấy chi tiết user module của người dùng hiện tại.
    /// </summary>
    /// <param name="moduleId">Mã module.</param>
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

    /// <summary>
    /// Cập nhật user module của người dùng hiện tại.
    /// </summary>
    /// <param name="moduleId">Mã module.</param>
    /// <param name="request">Thông tin user module cần cập nhật.</param>
    [HttpPut("{moduleId:guid}")]
    [SwaggerRequestExample(typeof(UpdateUserModuleRequestDTO), typeof(UpdateUserModuleRequestExample))]
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

    /// <summary>
    /// Xóa user module của người dùng hiện tại.
    /// </summary>
    /// <param name="moduleId">Mã module.</param>
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
