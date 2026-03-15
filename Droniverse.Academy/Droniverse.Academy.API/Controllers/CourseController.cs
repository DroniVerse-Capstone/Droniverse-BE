using Droniverse.Academy.Application.IService;
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
                _logger.LogError(ex, "CreateCourse failed");
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
                _logger.LogError(ex, "GetCourseByIdActive failed for {CourseId}", courseId);
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
                _logger.LogError(ex, "GetCourseByIdAll failed for {CourseId}", courseId);
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
                _logger.LogError(ex, "GetAllCoursesActive failed (pageIndex={PageIndex}, pageSize={PageSize})", pageIndex, pageSize);
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
                _logger.LogError(ex, "GetAllCoursesAll failed (pageIndex={PageIndex}, pageSize={PageSize})", pageIndex, pageSize);
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
                return Ok(SuccessResponse<object>.Create(null!, "Publish course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishCourse failed for {CourseId}", courseId);
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
                return Ok(SuccessResponse<object>.Create(null!, "Unpublish course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnpublishCourse failed for {CourseId}", courseId);
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
                _logger.LogError(ex, "DeleteCourse failed for {CourseId}", courseId);
                throw;
            }
        }
    }
}
