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
[ApiExplorerSettings(IgnoreApi = true)]
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
