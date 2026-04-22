using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers
{
    [Route("academy/level")]
    [ApiController]
    public class LevelController : ControllerBase
    {
        private readonly ILevelService _levelService;

        public LevelController(ILevelService levelService)
        {
            _levelService = levelService;
        }

        [HttpGet("GetLevelByDrone/{droneId}")]
        public async Task<IActionResult> GetLevelByDrone(Guid droneId)
        {
            var levels = await _levelService.GetLevelByDroneAsync(droneId);
            return Ok(SuccessResponse<IEnumerable<LevelMiniResponse>>.Create(levels, "Lấy chi tiết level thành công."));

        }

        [HttpPost("AddLevelCourse/{levelId:guid}")]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddLevelCourse(Guid levelId, [FromBody] LevelCoursesRequestDTO? request, CancellationToken cancellationToken = default)
        {
            if (levelId == Guid.Empty)
                return BadRequest(SuccessResponse<object>.Create(null!, "Level không hợp lệ."));

            var updatedCount = await _levelService.ReplaceLevelCoursesAsync(levelId, request?.CourseIds, cancellationToken);
            return Ok(SuccessResponse<object>.Create(new { created = updatedCount }, "Cập nhật level course thành công."));
        }
    }
    
}
