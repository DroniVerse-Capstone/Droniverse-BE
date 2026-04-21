using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers
{
    [Route("api/[controller]")]
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
    }
    
}
