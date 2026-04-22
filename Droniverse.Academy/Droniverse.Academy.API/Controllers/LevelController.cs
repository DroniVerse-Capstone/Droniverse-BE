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

        /// <summary>
        /// Lấy danh sách level theo drone.
        /// </summary>
        /// <param name="droneId">Mã drone cần truy vấn level.</param>
        /// <returns>Danh sách level của drone theo thứ tự tăng dần.</returns>
        // GET academy/level/GetLevelByDrone/{droneId}
        [HttpGet("GetLevelByDrone/{droneId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<LevelMiniResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLevelByDrone(Guid droneId)
        {
            var levels = await _levelService.GetLevelByDroneAsync(droneId);
            return Ok(SuccessResponse<IEnumerable<LevelMiniResponse>>.Create(levels, "Lấy chi tiết level thành công."));

        }

        /// <summary>
        /// Lấy level path của drone, gồm các level từ 1 đến 4 và danh sách course điều kiện của từng level.
        /// </summary>
        /// <param name="droneId">Mã drone cần truy vấn level path.</param>
        /// <param name="cancellationToken">Token hủy request.</param>
        /// <returns>Danh sách level path với course điều kiện theo từng level.</returns>
        // GET academy/level/GetLevelPath/{droneId}
        [HttpGet("GetLevelPath/{droneId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<LevelPathResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLevelPath(Guid droneId, CancellationToken cancellationToken = default)
        {
            var levelPath = await _levelService.GetLevelPathAsync(droneId, cancellationToken);
            return Ok(SuccessResponse<IEnumerable<LevelPathResponseDTO>>.Create(levelPath, "Lấy level path thành công."));
        }

        /// <summary>
        /// Thiết lập danh sách course điều kiện cho một level.
        /// </summary>
        /// <param name="levelId">Mã level cần cập nhật điều kiện course.</param>
        /// <param name="request">Danh sách course điều kiện của level.</param>
        /// <param name="cancellationToken">Token hủy request.</param>
        /// <returns>Số lượng course điều kiện đã được cập nhật.</returns>
        // POST academy/level/AddLevelCourse/{levelId}
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
