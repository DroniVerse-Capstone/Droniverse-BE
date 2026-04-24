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
    [Route("academy/user/enrollments/{enrollmentId:guid}/lessons/{lessonId:guid}/simulator")]
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
        /// Lấy trạng thái học simulator của người dùng theo lesson.
        /// Trả về dữ liệu user simulator và dữ liệu simulator (vr/web) tương ứng.
        /// </summary>
        /// <param name="enrollmentId">Mã enrollment của người học.</param>
        /// <param name="lessonId">Mã lesson.</param>
        [HttpGet]
        public async Task<IActionResult> GetSimulatorLearningState(Guid enrollmentId, Guid lessonId)
        {
            try
            {
                var result = await _service.GetSimulatorLearningStateAsync(enrollmentId, lessonId);
                return Ok(SuccessResponse<SimulatorLearningStateDTO>.Create(result, "Lấy dữ liệu simulator thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy dữ liệu simulator thất bại.");
                throw;
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit(Guid enrollmentId, Guid lessonId, [FromBody] SubmitSimulatorRequestDto request)
        {
            try
            {
                var result = await _service.SubmitSimulatorAsync(enrollmentId, lessonId, request.FlightTime, request.Score);
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
