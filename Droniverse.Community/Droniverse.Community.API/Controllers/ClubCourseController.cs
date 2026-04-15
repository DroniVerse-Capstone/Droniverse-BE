using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.API.Examples;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.API.Controllers
{
    /// <summary>
    /// API quản lý thông tin Câu lạc bộ (Club)
    /// Bao gồm các chức năng: lấy danh sách, lấy chi tiết, tạo mới, cập nhật,
    /// xóa, tham gia câu lạc bộ và lấy danh sách khóa học thuộc câu lạc bộ.
    /// </summary>
    [Route("community/clubs")]
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
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="request">Thông tin khóa học cần thêm (courseId, totalQuantity, profitType).</param>
        /// <returns>
        /// 200 OK - Thêm khóa học vào câu lạc bộ thành công.
        /// 400 BadRequest - Dữ liệu không hợp lệ.
        /// 409 Conflict - Khóa học đã tồn tại trong câu lạc bộ.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpPost("{clubId:guid}/courses")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(AddClubCourseRequest), typeof(AddClubCourseRequestMultipleExample))]
        public async Task<ApiResponse> AddCourseToClub(Guid clubId, [FromBody] AddClubCourseRequest request)
        {
            var added = await _clubCourseService.AddCourseToClub(clubId, request);
            return SuccessResponse<ClubCourseResponseDto>.Create(added, "Thêm khóa học vào câu lạc bộ thành công!");
        }

        /// <summary>
        /// Cập nhật thông tin khóa học trong câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="courseId">ID khóa học.</param>
        /// <param name="request">Thông tin cần cập nhật (totalQuantity/profitType).</param>
        /// <returns>
        /// 200 OK - Cập nhật thành công.
        /// 400 BadRequest - Dữ liệu không hợp lệ.
        /// 404 NotFound - Không tìm thấy ClubCourse.
        /// </returns>
        [HttpPut("{clubId:guid}/courses/{courseId:guid}")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(UpdateClubCourseRequest), typeof(UpdateClubCourseRequestMultipleExample))]
        public async Task<ApiResponse> UpdateClubCourse(Guid clubId, Guid courseId, [FromBody] UpdateClubCourseRequest request)
        {
            var updated = await _clubCourseService.UpdateClubCourse(clubId, courseId, request);
            return SuccessResponse<ClubCourseResponseDto>.Create(updated, "Cập nhật thông tin khóa học trong câu lạc bộ thành công!");
        }

        /// <summary>
        /// Tăng số lượng slot của khóa học trong câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="courseId">ID khóa học.</param>
        /// <param name="request">Số lượng slot cần tăng, phải lớn hơn 0.</param>
        /// <returns>
        /// 200 OK - Tăng slot thành công.
        /// 400 BadRequest - Dữ liệu không hợp lệ.
        /// 404 NotFound - Không tìm thấy ClubCourse.
        /// </returns>
        [HttpPatch("{clubId:guid}/courses/{courseId:guid}/increase-capacity")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> IncreaseCapacity(Guid clubId, Guid courseId, [FromBody] IncreaseClubCourseCapacityRequest request)
        {
            var result = await _clubCourseService.IncreaseCapacity(clubId, courseId, request);
            return SuccessResponse<ClubCourseResponseDto>.Create(result, "Tăng số lượng slot thành công!");
        }

        /// <summary>
        /// Consume slot khi người dùng đăng ký khóa học trong câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="courseId">ID khóa học.</param>
        /// <param name="request">Số lượng cần consume (mặc định = 1).</param>
        /// <returns>
        /// 200 OK - Consume slot thành công.
        /// 400 BadRequest - Dữ liệu không hợp lệ.
        /// 404 NotFound - Không tìm thấy ClubCourse.
        /// 409 Conflict - Không đủ slot còn lại.
        /// </returns>
        [HttpPost("{clubId:guid}/courses/{courseId:guid}/consume")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> ConsumeSlot(Guid clubId, Guid courseId, [FromBody] ChangeClubCourseSlotRequest? request)
        {
            var result = await _clubCourseService.ConsumeSlot(clubId, courseId, request);
            return SuccessResponse<ClubCourseResponseDto>.Create(result, "Consume slot thành công!");
        }


        /// <summary>
        /// Consume slot khi người dùng đăng ký khóa học trong câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="courseId">ID khóa học.</param>
        /// <param name="request">Số lượng cần consume (mặc định = 1).</param>
        /// <returns>
        /// 200 OK - Consume slot thành công.
        /// 400 BadRequest - Dữ liệu không hợp lệ.
        /// 404 NotFound - Không tìm thấy ClubCourse.
        /// 409 Conflict - Không đủ slot còn lại.
        /// </returns>
        [HttpPost("{clubId:guid}/courses/{courseId:guid}/consume-cross")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsumeSlotCross(Guid clubId, Guid courseId, [FromBody] ChangeClubCourseSlotRequest? request)
        {
            var result = await _clubCourseService.ConsumeSlot(clubId, courseId, request);
            return Ok(result);
        }

        /// <summary>
        /// Restore slot khi hủy đăng ký hoặc rollback thao tác.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="courseId">ID khóa học.</param>
        /// <param name="request">Số lượng cần restore (mặc định = 1).</param>
        /// <returns>
        /// 200 OK - Restore slot thành công.
        /// 400 BadRequest - Dữ liệu không hợp lệ.
        /// 404 NotFound - Không tìm thấy ClubCourse.
        /// 409 Conflict - Restore vượt quá tổng số lượng.
        /// </returns>
        [HttpPost("{clubId:guid}/courses/{courseId:guid}/restore")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> RestoreSlot(Guid clubId, Guid courseId, [FromBody] ChangeClubCourseSlotRequest? request)
        {
            var result = await _clubCourseService.RestoreSlot(clubId, courseId, request);
            return SuccessResponse<ClubCourseResponseDto>.Create(result, "Restore slot thành công!");
        }

        /// <summary>
        /// Lấy số lượng slot còn lại của khóa học trong câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="courseId">ID khóa học.</param>
        /// <returns>
        /// 200 OK - Lấy số lượng slot còn lại thành công.
        /// 404 NotFound - Không tìm thấy ClubCourse.
        /// </returns>
        [HttpGet("{clubId:guid}/courses/{courseId:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ClubCourseRemainingQuantityResponseDto> GetRemainingQuantity(Guid clubId, Guid courseId)
        {
            var result = await _clubCourseService.GetRemainingQuantity(clubId, courseId);
            return result;
        }

    }
}
