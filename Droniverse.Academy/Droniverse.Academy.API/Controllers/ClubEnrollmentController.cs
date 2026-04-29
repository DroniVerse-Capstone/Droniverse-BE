using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers
{

    [Route("api/club")]
    [ApiController]
    public class ClubEnrollmentController : ControllerBase
    {
        private readonly ILogger<AdminEnrollmentController> _logger;
        private readonly IAdminEnrollmentService _service;
        public ClubEnrollmentController(ILogger<AdminEnrollmentController> logger, IAdminEnrollmentService service)
        {
            _logger = logger;
            _service = service;
        }
        /// <summary>
        /// Lấy danh sách enrollment của một club
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
        /// <param name="userId">Lọc theo người dùng.</param>
        /// <param name="courseVersionId">Lọc theo phiên bản khóa học.</param>
        /// <param name="droneId">Lọc theo drone.</param>
        /// <param name="levelId">Lọc theo level.</param>
        /// <param name="status">Lọc theo trạng thái enrollment.</param>
        [HttpGet("{clubId:guid}/enrollment")]       
        public async Task<IActionResult> GetEnrollments(
        Guid clubId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? courseVersionId = null,
        [FromQuery] Guid? droneId = null,
        [FromQuery] Guid? levelId = null,
        [FromQuery] EnrollmentStatusFilter status = EnrollmentStatusFilter.All)
        {
            try
            {
                var result = await _service.GetEnrollmentsAsync(pageIndex, pageSize, userId, courseVersionId, droneId, levelId, clubId, MapEnrollmentStatus(status));
                return Ok(SuccessResponse<PaginationResult<IEnumerable<CoursesEnrollmentResponse>>>.Create(result, "Lấy danh sách enrollment thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách enrollment thất bại.");
                throw;
            }
        }
        private static EnrollStatus? MapEnrollmentStatus(EnrollmentStatusFilter status)
        {
            return status switch
            {
                EnrollmentStatusFilter.All => null,
                EnrollmentStatusFilter.Dropped => EnrollStatus.DROPPED,
                EnrollmentStatusFilter.Active => EnrollStatus.ACTIVE,
                EnrollmentStatusFilter.Completed => EnrollStatus.COMPLETED,
                EnrollmentStatusFilter.LimitedAccess => EnrollStatus.LIMITED_ACCESS,
                _ => null
            };
        }
    }
    }
