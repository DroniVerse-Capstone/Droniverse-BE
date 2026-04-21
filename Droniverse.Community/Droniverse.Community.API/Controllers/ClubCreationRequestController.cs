using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/club-creation-request")]
    [ApiController]
    [Authorize]
    public class ClubCreationRequestController : ControllerBase
    {
        private readonly IClubCreationRequestService _clubCreationRequestService;

        public ClubCreationRequestController(IClubCreationRequestService clubCreationRequestService)
        {
            _clubCreationRequestService = clubCreationRequestService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách yêu cầu tạo câu lạc bộ của hệ thống.
        /// </summary>
        /// <param name="searchRequest">Trạng thái của yêu cầu: 0-PENDING, 1-APPROVED, 2-REJECTED, 3-CANCEL. Để null để lấy tất cả.</param>
        /// <returns>
        /// 200 OK - Trả về danh sách yêu cầu tạo câu lạc bộ
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<ClubCreationRequestResponseDto>>>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetAllClubCreationRequests([FromQuery] ClubCreationRequestSearchRequest searchRequest)
        {
            return SuccessResponse<PaginationResult<IEnumerable<ClubCreationRequestResponseDto>>>.Create(
                await _clubCreationRequestService.GetAllClubCreationRequest(searchRequest),
                "Lấy toàn bộ danh sách yêu cầu tạo câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy danh sách yêu cầu tạo câu lạc bộ của quản lý hiện tại.
        /// </summary>
        /// <param name="status">Trạng thái của yêu cầu: 0-PENDING, 1-APPROVED, 2-REJECTED, 3-CANCEL. Để null để lấy tất cả.</param>
        /// <returns>
        /// 200 OK - Trả về danh sách yêu cầu tạo câu lạc bộ
        /// </returns>
        [HttpGet("my-requests")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ClubCreationRequestResponseDto>>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.ClubManager)]
        public async Task<ApiResponse> GetMyClubCreationRequest([FromQuery] ClubCreationRequestStatus? status = null)
        {
            return SuccessResponse<IEnumerable<ClubCreationRequestResponseDto>>.Create(
                await _clubCreationRequestService.GetMyClubCreationRequest(status),
                "Lấy danh sách yêu cầu tạo câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy thông tin chi tiết yêu cầu tạo câu lạc bộ theo ID.
        /// </summary>
        /// <param name="id">ClubCreationRequestID</param>
        /// <returns>
        /// 200 OK - Trả về thông tin chi tiết yêu cầu tạo câu lạc bộ
        /// 404 NotFound - Không tìm thấy yêu cầu
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCreationRequestResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> GetClubCreationRequestById(Guid id)
        {
            return SuccessResponse<ClubCreationRequestResponseDto>.Create(
                await _clubCreationRequestService.GetClubCreationRequestById(id),
                "Lấy thông tin yêu cầu tạo câu lạc bộ thành công!");
        }

        /// <summary>
        /// Tạo yêu cầu tạo câu lạc bộ mới
        /// </summary>
        /// <param name="request">Thông tin yêu cầu tạo câu lạc bộ</param>
        /// <returns>
        /// 201 Created - Tạo yêu cầu thành công
        /// 400 BadRequest - Dữ liệu không hợp lệ
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessResponse<ClubCreationRequestCreateResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(ClubCreationRequestCreateDto), typeof(ClubCreateRequestExample))]
        [Authorize(Roles = Roles.ClubManager)]
        public async Task<ApiResponse> CreateRequestToCreateClub([FromBody] ClubCreationRequestCreateDto request)
        {
            return SuccessResponse<ClubCreationRequestCreateResponseDto>.Create(
                await _clubCreationRequestService.CreateRequestToCreateClub(request),
                "Gửi yêu cầu tạo câu lạc bộ thành công");
        }

        /// <summary>
        /// API cập nhật trạng thái của request
        /// </summary>
        /// <param name="id">ClubCreationRequestID : 548accad-a382-4c63-8053-7cc621d25615</param>
        /// <param name="request"></param>
        /// <remarks>
        /// [0: PENDING, 1: APPROVED, 2: REJECTED, 3: CANCELLED]
        /// </remarks>
        /// <returns>
        /// 200 OK - Cập nhật trạng thái thành công
        /// 404 NotFound - Không tìm thấy yêu cầu
        /// </returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCreationRequestUpdateStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(ClubCreationRequestUpdateStatusDto), typeof(ClubCreationRequestUpdateStatusExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> UpdateRequestStatus(Guid id, [FromBody] ClubCreationRequestUpdateStatusDto request)
        {
            return SuccessResponse<ClubCreationRequestUpdateStatusResponseDto>.Create(
                await _clubCreationRequestService.UpdateRequestStatus(id, request),
                "Cập nhật trạng thái yêu cầu tạo câu lạc bộ thành công");
        }

        /// <summary>
        /// API cập nhật thông tin của request
        /// </summary>
        /// <param name="id">ClubCreationRequestID</param>
        /// <param name="request">Thông tin cần cập nhật</param>
        /// <remarks>
        /// Chỉ có thể cập nhật khi request đang ở trạng thái PENDING (0).
        /// Không thể cập nhật status thông qua API này.
        /// </remarks>
        /// <returns>
        /// 200 OK - Cập nhật thông tin thành công
        /// 404 NotFound - Không tìm thấy yêu cầu
        /// 400 BadRequest - Request không ở trạng thái PENDING
        /// </returns>
        [HttpPut("{id}/information")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCreationRequestUpdateInfoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(ClubCreationRequestUpdateInfoDto), typeof(ClubCreationRequestUpdateInfoExample))]
        [Authorize(Roles = Roles.ClubManager)]
        public async Task<ApiResponse> UpdateRequestInfo(Guid id, [FromBody] ClubCreationRequestUpdateInfoDto request)
        {
            return SuccessResponse<ClubCreationRequestUpdateInfoResponseDto>.Create(
                await _clubCreationRequestService.UpdateRequestInfo(id, request),
                "Cập nhật thông tin yêu cầu tạo câu lạc bộ thành công");
        }
    }
}
