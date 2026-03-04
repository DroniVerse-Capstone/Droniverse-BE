using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Extensions;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel;

namespace Droniverse.Community.API.Controllers
{
    /// <summary>
    /// API quản lý thông tin Câu lạc bộ (Club)
    /// Bao gồm các chức năng: lấy danh sách, lấy chi tiết, tạo mới, cập nhật,
    /// xóa, tham gia câu lạc bộ và lấy danh sách khóa học thuộc câu lạc bộ.
    /// </summary>
    [Route("community/clubs")]
    [ApiController]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _clubService;
        private readonly ICloudinaryService _cloudinaryService;
        public ClubController(IClubService clubService, ICloudinaryService cloudinaryService)
        {
            _clubService = clubService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload-temp-image")]
        public async Task<IActionResult> UploadTempImage([FromForm] FileUploadDto file)
        {
            return await this.UploadImageAsync(_cloudinaryService, file, "droniverse/temp");
        }

        /// <summary>
        /// Lấy toàn bộ danh sách câu lạc bộ
        /// </summary>
        /// <remarks>
        /// API trả về danh sách tất cả các club hiện có trong hệ thống.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về danh sách ClubResponseDto
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetAllCLubs()
        {
            try
            {
                IEnumerable<ClubResponseDto> clubs = await _clubService.GetAllClubs();
                return SuccessResponse<IEnumerable<ClubResponseDto>>
                    .Create(clubs, "Lấy danh sách câu lạc bộ thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER1001");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một câu lạc bộ theo ID
        /// </summary>
        /// <param name="id">GUID định danh của câu lạc bộ</param>
        /// <remarks>
        /// Nếu không tìm thấy club theo ID truyền vào, service có thể throw exception.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về thông tin chi tiết ClubResponseDto  
        /// 404 NotFound - Nếu không tồn tại club
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetClubById(
            [FromRoute]
            [DefaultValue("32aca866-566c-4ddf-b9ba-5640724ecdee")]
            Guid id)
        {
            try
            {
                ClubResponseDto club = await _clubService.GetClubById(id);
                return SuccessResponse<ClubResponseDto>
                    .Create(club, $"Lấy thông tin câu lạc bộ với ID [{id}] thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }

        /// <summary>
        /// Tạo mới một câu lạc bộ
        /// </summary>
        /// <param name="clubRequest">Thông tin dữ liệu tạo mới club</param>
        /// <remarks>
        /// Sau khi tạo thành công sẽ trả về HTTP 201 và đường dẫn tới API lấy chi tiết club.
        /// </remarks>
        /// <returns>
        /// 201 Created - Tạo thành công và trả về dữ liệu ClubResponseDto  
        /// 400 BadRequest - Nếu dữ liệu đầu vào không hợp lệ
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(ClubAttemptRequestCreateDto), typeof(ClubCreateMultipleExample))]
        public async Task<ApiResponse> CreateClub([FromBody] ClubCreateDto clubRequest)
        {
            try
            {
                ClubResponseDto createdClub = await _clubService.CreateClub(clubRequest);
                return SuccessResponse<ClubResponseDto>
                    .Create(createdClub, "Tạo câu lạc bộ thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }

        /// <summary>
        /// Cập nhật thông tin câu lạc bộ theo ID
        /// </summary>
        /// <param name="id">GUID của câu lạc bộ cần cập nhật</param>
        /// <param name="clubRequest">Dữ liệu cập nhật</param>
        /// Body: 
        /// <returns>
        /// 200 OK - Cập nhật thành công và trả về ClubResponseDto  
        /// 400 BadRequest - Nếu dữ liệu không hợp lệ  
        /// 404 NotFound - Nếu không tồn tại club
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(ClubUpdateDto), typeof(ClubUpdateExample))]
        public async Task<ApiResponse> UpdateClub(Guid id, [FromBody] ClubUpdateDto clubRequest)
        {
            try
            {
                ClubResponseDto updatedClub = await _clubService.UpdateClub(id, clubRequest);
                return SuccessResponse<ClubResponseDto>
                    .Create(updatedClub, $"Cập nhật câu lạc bộ với ID [{id}] thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }

        /// <summary>
        /// Xóa câu lạc bộ theo ID
        /// </summary>
        /// <param name="id">GUID của câu lạc bộ cần xóa</param>
        /// <returns>
        /// 204 NoContent - Xóa thành công
        /// 500 InternalServerError - Nếu có lỗi xảy ra khi xóa
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> DeleteClub(Guid id)
        {
            try
            {
                bool isDeleted = await _clubService.DeleteClub(id);

                if (!isDeleted)
                {
                    return ErrorResponse.Create("Xóa câu lạc bộ thất bại!", "Err91");
                }

                return SuccessResponse<string>.Create(null, $"Xóa câu lạc bộ với ID [{id}] thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }

        /// <summary>
        /// Gửi yêu cầu tham gia câu lạc bộ
        /// </summary>
        /// <param name="request">Thông tin yêu cầu tham gia club</param>
        /// <remarks>
        /// Nếu club là public có thể được tham gia trực tiếp.  
        /// Nếu club là private có thể tạo một yêu cầu chờ duyệt.
        /// </remarks>
        /// <example>
        /// A41YQ1
        /// </example>
        /// <returns>
        /// 200 OK - Trả về thông tin club sau khi xử lý tham gia
        /// </returns>
        [HttpPost("attemption")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ApiResponse> JoinClub([FromBody] ClubJoinDto request)
        {
            string message = "Tham gia câu lạc bộ thành công";
            JoinClubResponse response = await _clubService.JoinClub(request);
            if (!response.ClubIsPublic)
                message = "Tạo yêu cầu tham gia thành công, vui lòng đợi được duyệt !";
            return SuccessResponse<JoinClubResponse>
                .Create(response, message);

        }

        /// <summary>
        /// Lấy danh sách khóa học thuộc một câu lạc bộ
        /// </summary>
        /// <param name="id">GUID của câu lạc bộ</param>
        /// <returns>
        /// 200 OK - Trả về danh sách khóa học  
        /// 404 NotFound - Nếu không tồn tại câu lạc bộ  
        /// 500 InternalServerError - Nếu xảy ra lỗi hệ thống
        /// </returns>
        [HttpGet("{id}/courses")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> GetClubCourses(Guid id, [FromQuery] ClubCourseSearchRequest searchRequest)
        {
            try
            {
                var courses = await _clubService.GetClubCourses(id, searchRequest);
                return SuccessResponse<IEnumerable<CourseResponseDto>>
                    .Create(courses, "Lấy danh sách khóa học của câu lạc bộ thành công!");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse.Create(ex.Message, "ER404");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }
    }
}