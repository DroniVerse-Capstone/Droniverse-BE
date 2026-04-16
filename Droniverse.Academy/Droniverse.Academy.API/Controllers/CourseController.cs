using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.API.Enums;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.DTOs.Request;
using Swashbuckle.AspNetCore.Filters;

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
        /// <returns>Thông tin khóa học vừa được tạo.</returns>
        // POST academy/courses
        [HttpPost]
        [ProducesResponseType(typeof(SuccessResponse<CourseDetailResponseDTO>), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCourse()
        {
            // Vì yêu cầu chỉ cần tạo mới khóa học với trạng thái mặc định là DRAFT và không cần input gì thêm, nên API này sẽ không nhận body nào cả.
            try
            {
                var created = await _courseService.CreateCourseAsync();
                return CreatedAtAction(
                    nameof(GetCourseById),
                    new { courseId = created.CourseID },
                    SuccessResponse<CourseDetailResponseDTO>.Create(created, "Tạo course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tạo khóa học thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học theo danh sách ID (kèm filter + pagination).
        /// </summary>
        /// <param name="request">Danh sách <c>CourseId</c> cần truy vấn.</param>
        /// <param name="searchRequest">Bộ lọc + phân trang.</param>
        /// <returns>Danh sách khóa học tương ứng với các ID được gửi lên.</returns>
        // POST academy/courses/by-ids
        [HttpPost("by-ids")]
        [SwaggerRequestExample(typeof(GetCoursesByIdsRequestDTO), typeof(GetCoursesByIdsExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CoursesByIdsSuccessResponseExample))]
        [ProducesResponseType(typeof(PagedCourseBulkResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCoursesByIds(
            [FromQuery] CourseBulkSearchRequest searchRequest,
            [FromBody] GetCoursesByIdsRequestDTO request)
        {
            try
            {
                var result = await _courseService.GetCoursesByIdsAsync(searchRequest, request.CourseIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách course theo id thất bại.");
                throw;
            }
        }

        [HttpPost("by-ids/management")]
        [SwaggerRequestExample(typeof(GetCoursesByIdsRequestDTO), typeof(GetCoursesByIdsExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CoursesByIdsSuccessResponseExample))]
        [ProducesResponseType(typeof(PagedCourseBulkResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCoursesByIdsManagement(
         [FromQuery] ManagerCourseBulkSearchRequest searchRequest,
         [FromBody] GetCoursesByIdsRequestDTO request)
        {
            try
            {
                var result = await _courseService.GetCoursesByIdsManagementAsync(searchRequest, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách course theo id thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học đơn giản theo danh sách ID.
        /// </summary>
        /// <param name="request">Danh sách <c>CourseId</c> cần truy vấn.</param>
        /// <returns>Danh sách khóa học rút gọn tương ứng với các ID hợp lệ.</returns>
        [HttpPost("by-ids/simple")]
        [SwaggerRequestExample(typeof(GetCoursesByIdsRequestDTO), typeof(GetCoursesByIdsExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SimpleCoursesByIdsSuccessResponseExample))]
        [ProducesResponseType(typeof(IEnumerable<SimpleCourseResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCoursesByIdsSimple(
        [FromBody] GetCoursesByIdsRequestDTO request)
        {
            try
            {
                var result = await _courseService.GetCoursesByIdsSimpleAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách khóa học đơn giản theo ID thất bại.");
                throw;
            }
        }

        ///// <summary>
        ///// Lấy danh sách khóa học theo danh sách ID (kèm filter + pagination).
        ///// </summary>
        ///// <param name="request">Danh sách <c>CourseId</c> cần truy vấn.</param>
        ///// <param name="searchRequest">Bộ lọc + phân trang.</param>
        ///// <returns>Danh sách khóa học tương ứng với các ID được gửi lên.</returns>
        //// POST academy/courses/by-ids
        //[HttpPost("by-ids")]
        //[SwaggerRequestExample(typeof(GetCoursesByIdsRequestDTO), typeof(GetCoursesByIdsExample))]
        //[SwaggerResponseExample(StatusCodes.Status200OK, typeof(CoursesByIdsSuccessResponseExample))]
        //[ProducesResponseType(typeof(SuccessResponse<PagedCourseBulkResponse>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> GetCoursesByIds(
        //    [FromQuery] CourseBulkSearchRequest searchRequest,
        //    [FromBody] GetCoursesByIdsRequestDTO request)
        //{
        //    try
        //    {
        //        var result = await _courseService.GetCoursesByIdsAsync(searchRequest, request.CourseIds);
        //        return Ok(SuccessResponse<PagedCourseBulkResponse>.Create(result, "Lấy danh sách course theo id thành công."));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lấy danh sách course theo id thất bại.");
        //        throw;
        //    }
        //}

        /// <summary>
        /// Lấy danh sách khóa học hot theo danh sách ID (sắp xếp theo độ hot và có phân trang).
        /// </summary>
        /// <param name="request">Danh sách <c>CourseId</c> cần truy vấn.</param>
        /// <param name="searchRequest">Thông tin phân trang.</param>
        /// <returns>Danh sách khóa học hot theo trang hiện tại.</returns>
        // POST academy/courses/by-ids/hot
        [HttpPost("by-ids/hot")]
        [SwaggerRequestExample(typeof(GetCoursesByIdsRequestDTO), typeof(GetCoursesByIdsExample))]
        [ProducesResponseType(typeof(SuccessResponse<PagedCourseBulkResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHotCoursesByIds(
            [FromQuery] HotCoursesSearchRequest searchRequest,
            [FromBody] GetCoursesByIdsRequestDTO request)
        {
            try
            {
                var result = await _courseService.GetHotCoursesByIdsAsync(searchRequest, request.CourseIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách khóa học hot theo id thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Lấy chi tiết khóa học (luôn trả về theo current version).
        /// </summary>
        // GET academy/courses/{courseId}
        [HttpGet("{courseId:guid}")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CourseDetailSuccessResponseExample))]
        [ProducesResponseType(typeof(SuccessResponse<CourseResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCourseById(Guid courseId)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(courseId);
                return Ok(SuccessResponse<CourseResponseDTO>.Create(course, "Lấy chi tiết course thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy chi tiết khóa học thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Lấy tổng quan khóa học theo phiên bản hiện tại.
        /// </summary>
        // GET academy/courses/{courseId}/overview
        [HttpGet("{courseId:guid}/overview")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CourseOverviewSuccessResponseExample))]
        [ProducesResponseType(typeof(SuccessResponse<CourseOverviewResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCourseOverview(
            [FromQuery] Guid clubId,
            Guid courseId,
            CancellationToken cancellationToken)
        {
            try
            {
                var overview = await _courseService.GetCourseOverviewAsync(clubId, courseId, cancellationToken);
                return Ok(SuccessResponse<CourseOverviewResponseDTO>.Create(overview, "Lấy tổng quan khóa học thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy tổng quan khóa học thất bại.");
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học (theo current version) và lọc theo trạng thái khóa học.
        /// </summary>
        // GET academy/courses?pageIndex=1&pageSize=10&search=...&status=All|Draft|Publish|Unpublish|Archived
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<CourseResponseDTO>>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCourses([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] CourseStatusFilter status = CourseStatusFilter.All)
        {
            try
            {
                var result = await _courseService.GetAllCoursesAsync(pageIndex, pageSize, search, MapToCourseStatus(status));
                return Ok(SuccessResponse<PaginationResult<IEnumerable<CourseResponseDTO>>>.Create(result, "Lấy danh sách course thành công."));
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
        /// <param name="courseId">Mã khóa học cần xuất bản.</param>
        // POST academy/courses/{courseId}/publish
        [HttpPost("{courseId}/publish")]
        [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PublishCourse(Guid courseId)
        {
            try
            {
                await _courseService.PublishCourseAsync(courseId);
                return Ok(SuccessResponse<string>.Create(string.Empty, "Xuất bản khóa học thành công."));
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
        /// <param name="courseId">Mã khóa học cần hủy xuất bản.</param>
        // POST academy/courses/{courseId}/unpublish
        [HttpPost("{courseId}/unpublish")]
        [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UnpublishCourse(Guid courseId)
        {
            try
            {
                await _courseService.UnpublishCourseAsync(courseId);
                return Ok(SuccessResponse<string>.Create(string.Empty, "Hủy xuất bản khóa học thành công."));
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
        [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCourse(Guid courseId)
        {
            try
            {
                await _courseService.DeleteCourseAsync(courseId);
                return Ok(SuccessResponse<string>.Create(string.Empty, "Xóa course thành công."));
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
