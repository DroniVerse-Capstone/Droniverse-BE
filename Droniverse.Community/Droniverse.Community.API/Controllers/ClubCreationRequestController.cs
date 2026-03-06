using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/club-creation-request")]
    [ApiController]
    public class ClubCreationRequestController : ControllerBase
    {
        private readonly IClubCreationRequestService _clubCreationRequestService;

        public ClubCreationRequestController(IClubCreationRequestService clubCreationRequestService)
        {
            _clubCreationRequestService = clubCreationRequestService;
        }

        /// <summary>
        /// Lấy danh sách yêu cầu tạo câu lạc bộ của quản lý hiện tại.
        /// </summary>
        /// <param name="status">Trạng thái của yêu cầu: 0-PENDING, 1-APPROVED, 2-REJECTED, 3-CANCEL. Để null để lấy tất cả.</param>
        [HttpGet("my-requests")]
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
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetClubCreationRequestById(Guid id)
        {
            return SuccessResponse<ClubCreationRequestResponseDto>.Create(
                await _clubCreationRequestService.GetClubCreationRequestById(id),
                "Lấy thông tin yêu cầu tạo câu lạc bộ thành công!");
        }

        [HttpPost]
        [SwaggerRequestExample(typeof(ClubCreationRequestCreateDto), typeof(ClubCreateRequestExample))]
        public async Task<ApiResponse> CreateRequestToCreateClub([FromBody] ClubCreationRequestCreateDto request)
        {
            return SuccessResponse<ClubCreationRequestCreateResponseDto>.Create(
                await _clubCreationRequestService.CreateRequestToCreateClub(request),
                "Gửi yêu cầu tạo câu lạc bộ thành công");
        }

        /// <summary>
        /// API cập nhật trạng thái của request
        /// </summary>
        /// <param name="id">ClubCreationRequestID : f5364a01-da2f-4543-a850-3cf49d14e174</param>
        /// <param name="request"></param>
        /// <remarks>
        /// [0: PENDING, 1: APPROVED, 2: REJECTED, 3: CANCELLED]
        /// </remarks>
        /// <returns></returns>
        [HttpPut("{id}/status")]
        [SwaggerRequestExample(typeof(ClubCreationRequestUpdateStatusDto), typeof(ClubCreationRequestUpdateStatusExample))]
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
        /// <returns></returns>
        [HttpPut("{id}/information")]
        [SwaggerRequestExample(typeof(ClubCreationRequestUpdateInfoDto), typeof(ClubCreationRequestUpdateInfoExample))]
        public async Task<ApiResponse> UpdateRequestInfo(Guid id, [FromBody] ClubCreationRequestUpdateInfoDto request)
        {
            return SuccessResponse<ClubCreationRequestUpdateInfoResponseDto>.Create(
                await _clubCreationRequestService.UpdateRequestInfo(id, request),
                "Cập nhật thông tin yêu cầu tạo câu lạc bộ thành công");
        }
    }
}
