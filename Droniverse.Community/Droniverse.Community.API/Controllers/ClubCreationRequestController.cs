using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
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
    }
}
