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
                "Gửi yêu cầu tạo câu lạc bộ thành công thành công tới");
        }
    }
}
