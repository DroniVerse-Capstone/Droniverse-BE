using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/levels")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserLevelController : ControllerBase
{
    private readonly ILogger<UserLevelController> _logger;
    private readonly IUserLevelService _service;

    public UserLevelController(ILogger<UserLevelController> logger, IUserLevelService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy tất cả level đã đạt được của người dùng.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserLevels([FromQuery] Guid userId)
    {
        try
        {
            var levels = await _service.GetUserLevelsAsync(userId);
            return Ok(SuccessResponse<IEnumerable<LevelMiniResponse>>.Create(levels, "Lấy danh sách level của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách level của người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy level cao nhất của người dùng theo từng drone.
    /// </summary>
    [HttpGet("max")]
    public async Task<IActionResult> GetMaxUserLevels([FromQuery] Guid userId)
    {
        try
        {
            var levels = await _service.GetMaxUserLevelsAsync(userId);
            return Ok(SuccessResponse<IEnumerable<LevelMiniResponse>>.Create(levels, "Lấy level cao nhất theo từng drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy level cao nhất theo từng drone thất bại.");
            throw;
        }
    }
}
