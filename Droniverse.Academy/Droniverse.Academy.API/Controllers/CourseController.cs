using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.DTO.Request;
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

        // POST academy/courses
        [HttpPost]
        public async Task<IActionResult> CreateCourse()
        {
            try
            {
                var created = await _courseService.CreateCourseAsync();
                return CreatedAtAction(nameof(GetCourseByIdAll), new { courseId = created.CourseID }, SuccessResponse<object>.Create(created, "Tạo course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tạo khóa học thất bại.");
                throw;
            }
        }

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

        // GET academy/courses/{courseId}/active
        [HttpGet("{courseId}/active")]
        public async Task<IActionResult> GetCourseByIdActive(Guid courseId)
        {
            try
            {
                var course = await _courseService.GetCourseByIdActiveAsync(courseId);
                return Ok(SuccessResponse<object>.Create(course, "Lấy chi tiết course active thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy chi tiết khóa học đang hoạt động thất bại.");
                throw;
            }
        }

        // GET academy/courses/{courseId}/all
        [HttpGet("{courseId}/all")]
        public async Task<IActionResult> GetCourseByIdAll(Guid courseId)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAllAsync(courseId);
                return Ok(SuccessResponse<object>.Create(course, "Lấy chi tiết course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy chi tiết khóa học thất bại.");
                throw;
            }
        }

        // GET academy/courses/active?pageIndex=1&pageSize=10&search=...
        [HttpGet("active")]
        public async Task<IActionResult> GetAllCoursesActive([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var result = await _courseService.GetAllCoursesActiveAsync(pageIndex, pageSize, search);
                return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách course active thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách khóa học đang hoạt động thất bại.");
                throw;
            }
        }

        // GET academy/courses/all?pageIndex=1&pageSize=10&search=...
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCoursesAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var result = await _courseService.GetAllCoursesAllAsync(pageIndex, pageSize, search);
                return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách khóa học thất bại.");
                throw;
            }
        }

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
    }
}
