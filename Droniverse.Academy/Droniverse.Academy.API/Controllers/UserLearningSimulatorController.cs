using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Droniverse.Academy.API.Controllers
{
    [Route("academy/user/lessons/{userLessonId:guid}/simulator")]
    [ApiController]
    [Authorize(Roles = Roles.AllRoles)]
    public class UserLearningSimulatorController : ControllerBase
    {
        private readonly ILogger<UserLearningSimulatorController> _logger;
        private readonly IUserSimulatorService _service;

        public UserLearningSimulatorController(ILogger<UserLearningSimulatorController> logger, IUserSimulatorService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// Lấy trạng thái học simulator của người dùng theo user lesson.
        /// Trả về thông tin simulator và dữ liệu user simulator nếu đã có.
        /// </summary>
        /// <param name="userLessonId">Mã user lesson.</param>
        [HttpGet]
        public async Task<IActionResult> GetSimulatorLearningState(Guid userLessonId)
        {
            try
            {
                var result = await _service.GetSimulatorLearningStateAsync(userLessonId);
                return Ok(SuccessResponse<SimulatorLearningStateDTO>.Create(result, "Lấy dữ liệu simulator thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy dữ liệu simulator thất bại.");
                throw;
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit(Guid userLessonId, [FromBody] SubmitSimulatorRequestDto request)
        {
            try
            {
                var result = await _service.SubmitSimulatorAsync(userLessonId, request.FlightTime, request.Score);
                return Ok(SuccessResponse<bool>.Create(result, result ? "Submit thành công" : "Submit thất bại"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Submit simulator thất bại.");
                throw;
            }
        }
    }
}
