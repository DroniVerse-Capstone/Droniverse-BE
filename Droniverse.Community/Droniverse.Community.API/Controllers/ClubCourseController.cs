using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    /// <summary>
    /// API quản lý thông tin Câu lạc bộ (Club)
    /// Bao gồm các chức năng: lấy danh sách, lấy chi tiết, tạo mới, cập nhật,
    /// xóa, tham gia câu lạc bộ và lấy danh sách khóa học thuộc câu lạc bộ.
    /// </summary>
    [Route("community/club-course")]
    [ApiController]
    [Authorize]
    public class ClubCourseController : ControllerBase
    {
        private readonly IClubCourseService _clubCourseService;
        public ClubCourseController(IClubCourseService clubCourseService)
        {
            _clubCourseService = clubCourseService;
        }

        /// <summary>
        /// Thêm khóa học vào câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID của câu lạc bộ.</param>
        /// <param name="courseId">ID của khóa học.</param>
        /// <returns>
        /// 200 OK - Thêm thành công.
        /// 400 BadRequest - Dữ liệu không hợp lệ hoặc khóa học đã tồn tại trong câu lạc bộ.
        /// </returns>
        [HttpPost("{clubId:guid}/courses/{courseId:guid}")]
        [ProducesResponseType(typeof(SuccessResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> AddCourseToClub(Guid clubId, Guid courseId, [FromBody] AddClubCourseRequest request)
        {
            var added = await _clubCourseService.AddCourseToClub(clubId, courseId, request);

            if (!added)
                return ErrorResponse.Create("Khóa học đã tồn tại trong câu lạc bộ.", "CLUB_COURSE_EXISTS");

            return SuccessResponse<bool>.Create(true, "Thêm khóa học vào câu lạc bộ thành công!");
        }
    }
}
