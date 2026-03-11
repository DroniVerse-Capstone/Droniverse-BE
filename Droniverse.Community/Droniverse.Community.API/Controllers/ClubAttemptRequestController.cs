using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    [ApiController]
    [Route("community/club-attempt-request")]
    [Authorize]
    public class ClubAttemptRequestController : ControllerBase
    {
        private readonly IClubAttemptRequestService _clubAttemptRequestService;
        private readonly ICurrentUserService _currentUserService;

        public ClubAttemptRequestController(
            IClubAttemptRequestService clubAttemptRequestService, 
            ICurrentUserService currentUserService)
        {
            _clubAttemptRequestService = clubAttemptRequestService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Lấy danh sách yêu cầu tham gia câu lạc bộ của người dùng hiện tại
        /// </summary>
        /// <returns>
        /// 200 OK - Trả về danh sách yêu cầu tham gia câu lạc bộ
        /// </returns>
        [HttpGet("my-requests")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ClubRequestResponseDto>>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> GetMyClubAttemptRequests()
        {
            return SuccessResponse<IEnumerable<ClubRequestResponseDto>>.Create(
                await _clubAttemptRequestService.GetClubAttemptRequestsByRequester(),
                "Lấy danh sách yêu cầu tham gia câu lạc bộ thành công");
        }

        /// <summary>
        /// Lấy tất cả yêu cầu tham gia câu lạc bộ với filter/search (Admin & Manager only)
        /// </summary>
        /// <param name="searchRequest">Tham số search/filter</param>
        /// <remarks>
        /// **API dành cho ADMIN và SYSTEM_MANAGER để quản lý tất cả requests**
        /// 
        /// ### Search/Filter Parameters:
        /// 
        /// | Parameter | Type | Mô tả |
        /// |-----------|------|-------|
        /// | `status` | int? | 0=REJECT, 1=PENDING, 2=APPROVED |
        /// | `clubID` | Guid? | Filter theo Club |
        /// | `requesterID` | Guid? | Filter theo người gửi request |
        /// | `createdFrom` | DateTime? | Filter từ ngày tạo |
        /// | `createdTo` | DateTime? | Filter đến ngày tạo |
        /// | `processedFrom` | DateTime? | Filter từ ngày xử lý |
        /// | `processedTo` | DateTime? | Filter đến ngày xử lý |
        /// | `page` | int | Số trang (default = 1) |
        /// | `pageSize` | int | Số items/trang (default = 10) |
        /// | `sortBy` | string | CreatedAt, ProcessedAt (default = CreatedAt) |
        /// | `sortDirection` | string | asc, desc (default = desc) |
        /// 
        /// ### Ví dụ:
        /// ```
        /// GET /community/club-attempt-request?status=1&page=1&pageSize=20
        /// GET /community/club-attempt-request?createdFrom=2024-01-01&createdTo=2024-12-31
        /// GET /community/club-attempt-request?clubID=xxx&status=2
        /// ```
        /// </remarks>
        /// <returns>200 OK - Trả về danh sách requests với pagination</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<ClubRequestResponseDto>>>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> GetAllClubAttemptRequests([FromQuery] ClubAttemptRequestSearchRequest searchRequest)
        {
            var result = await _clubAttemptRequestService.GetAllClubAttemptRequests(searchRequest);
            
            return SuccessResponse<PaginationResult<IEnumerable<ClubRequestResponseDto>>>.Create(
                result,
                $"Lấy danh sách yêu cầu tham gia câu lạc bộ thành công! (Trang {result.PageIndex}/{result.TotalPages})");
        }

        /// <summary>
        /// Cập nhật trạng thái yêu cầu tham gia câu lạc bộ
        /// </summary>
        /// <param name="id">ID của yêu cầu tham gia</param>
        /// <param name="request">Thông tin cập nhật trạng thái</param>
        /// <returns>
        /// 200 OK - Cập nhật trạng thái thành công
        /// 404 NotFound - Không tìm thấy yêu cầu
        /// </returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(SuccessResponse<ClubAttemptRequestUpdateStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(ClubAttemptRequestUpdateStatusDto), typeof(ClubAttemptRequestUpdateStatusExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> UpdateRequestStatus(Guid id, [FromBody] ClubAttemptRequestUpdateStatusDto request)
        {
            return SuccessResponse<ClubAttemptRequestUpdateStatusResponseDto>.Create(
                await _clubAttemptRequestService.UpdateRequestStatus(id, request),
                "Cập nhật trạng thái yêu cầu tham gia câu lạc bộ thành công");
        }
    }
}
