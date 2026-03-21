using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Droniverse.Academy.API.Controllers
{
    [Route("academy/courses")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ILogger<CourseController> _logger;
        private readonly ICourseService _courseService;

        public CourseController(ILogger<CourseController> logger, ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        /// <summary>
        /// Tạo mới một khóa học.
        /// </summary>
        // POST academy/courses
        //
        [HttpPost]
        public async Task<IActionResult> CreateCourse()
        {
            try
            {
                var created = await _courseService.CreateCourseAsync();
                return CreatedAtAction(nameof(GetCourseById), new { courseId = created.CourseID }, SuccessResponse<object>.Create(created, "Tạo course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tạo khóa học thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học theo danh sách ID.
        /// </summary>
        // POST academy/courses/by-ids
        [HttpPost("by-ids")]
        public async Task<IActionResult> GetCoursesByIds([FromBody] GetCoursesByIdsRequestDTO request)
        {
            try
            {
                var result = await _courseService.GetCoursesByIdsAsync(request.CourseIds);
                return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách course theo id thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách course theo id thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Lấy chi tiết khóa học (luôn trả về theo current version).
        /// </summary>
        // GET academy/courses/{courseId}
        [HttpGet("{courseId:guid}")]
        public async Task<IActionResult> GetCourseById(Guid courseId)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(courseId);
                return Ok(SuccessResponse<object>.Create(course, "Lấy chi tiết course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy chi tiết khóa học thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học (theo current version) và lọc theo trạng thái khóa học.
        /// </summary>
        // GET academy/courses?pageIndex=1&pageSize=10&search=...&status=All|Draft|Publish|Unpublish|Archived
        [HttpGet]
        public async Task<IActionResult> GetAllCourses([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] CourseStatusFilter status = CourseStatusFilter.All)
        {
            try
            {
                var result = await _courseService.GetAllCoursesAsync(pageIndex, pageSize, search, MapToCourseStatus(status));
                return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách khóa học thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Xuất bản khóa học.
        /// </summary>
        // POST academy/courses/{courseId}/publish
        [HttpPost("{courseId}/publish")]
        public async Task<IActionResult> PublishCourse(Guid courseId)
        {
            try
            {
                await _courseService.PublishCourseAsync(courseId);
                return Ok(SuccessResponse<object>.Create(null!, "Xuất bản khóa học thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Xuất bản khóa học thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Hủy xuất bản khóa học.
        /// </summary>
        // POST academy/courses/{courseId}/unpublish
        [HttpPost("{courseId}/unpublish")]
        public async Task<IActionResult> UnpublishCourse(Guid courseId)
        {
            try
            {
                await _courseService.UnpublishCourseAsync(courseId);
                return Ok(SuccessResponse<object>.Create(null!, "Hủy xuất bản khóa học thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hủy xuất bản khóa học thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Xóa (archive) khóa học.
        /// </summary>
        // DELETE academy/courses/{courseId}
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourse(Guid courseId)
        {
            try
            {
                await _courseService.DeleteCourseAsync(courseId);
                return Ok(SuccessResponse<object>.Create(null!, "Xóa course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Xóa khóa học thất bại.");
                throw;
            }
        }

        private static CourseStatus? MapToCourseStatus(CourseStatusFilter status)
        {
            return status switch
            {
                CourseStatusFilter.All => null,
                CourseStatusFilter.Draft => CourseStatus.DRAFT,
                CourseStatusFilter.Publish => CourseStatus.PUBLISH,
                CourseStatusFilter.Unpublish => CourseStatus.UNPUBLISH,
                CourseStatusFilter.Archived => CourseStatus.ARCHIVED,
                _ => null
            };
        }
    }
}
