using Droniverse.Academy.API.Enums;
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

    /// <summary>
    /// Lấy danh sách user lab của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="completion">Bộ lọc trạng thái hoàn thành.</param>
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

    /// <summary>
    /// Lấy chi tiết user lab của người dùng hiện tại.
    /// </summary>
    /// <param name="userLabId">Mã user lab.</param>
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
