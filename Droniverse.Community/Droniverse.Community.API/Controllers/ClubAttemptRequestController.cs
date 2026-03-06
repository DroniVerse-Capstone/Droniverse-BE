using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    [ApiController]
    [Route("community/club-attempt-request")]
    public class ClubAttemptRequestController : ControllerBase
    {
        private readonly IClubAttemptRequestService _clubAttemptRequestService;

        public ClubAttemptRequestController(IClubAttemptRequestService clubAttemptRequestService)
        {
            _clubAttemptRequestService = clubAttemptRequestService;
        }

        [HttpGet("my-requests")]
        public async Task<ApiResponse> GetMyClubAttemptRequests()
        {
            // Temporary: fix RequesterId
            var requesterId = Guid.Parse("3197734d-d25d-42b1-b968-84b6ee4d33c2");

            return SuccessResponse<IEnumerable<ClubRequestResponseDto>>.Create(
                await _clubAttemptRequestService.GetClubAttemptRequestsByRequester(requesterId),
                "Lấy danh sách yêu cầu tham gia câu lạc bộ thành công");
        }

        [HttpPut("{id}/status")]
        [SwaggerRequestExample(typeof(ClubAttemptRequestUpdateStatusDto), typeof(ClubAttemptRequestUpdateStatusExample))]
        public async Task<ApiResponse> UpdateRequestStatus(Guid id, [FromBody] ClubAttemptRequestUpdateStatusDto request)
        {
            return SuccessResponse<ClubAttemptRequestUpdateStatusResponseDto>.Create(
                await _clubAttemptRequestService.UpdateRequestStatus(id, request),
                "Cập nhật trạng thái yêu cầu tham gia câu lạc bộ thành công");
        }
    }
}
