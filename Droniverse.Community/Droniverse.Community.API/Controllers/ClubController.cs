using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/clubs")]
    [ApiController]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _clubService;
        private readonly IClubRequestService _clubRequestService;

        public ClubController(IClubService clubService, IClubRequestService clubRequestService)
        {
            _clubService = clubService;
            _clubRequestService = clubRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCLubs()
        {
            IEnumerable<ClubResponseDto> clubs = await _clubService.GetAllClubs();
            return Ok(clubs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClubById(Guid id)
        {
            ClubResponseDto club = await _clubService.GetClubById(id);
            return Ok(club);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClub([FromBody] ClubCreateDto clubRequest)
        {
            ClubResponseDto createdClub = await _clubService.CreateClub(clubRequest);
            return CreatedAtAction(nameof(GetClubById), new { id = createdClub.ClubID }, createdClub);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClub(Guid id, [FromBody] ClubUpdateDto clubRequest)
        {
            ClubResponseDto updatedClub = await _clubService.UpdateClub(id, clubRequest);
            return Ok(updatedClub);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClub(Guid id)
        {
            bool isDeleted = await _clubService.DeleteClub(id);
            if (isDeleted)
            {
                return NoContent();
            }
            return StatusCode(500, "An error occurred while deleting the club.");
        }

        [HttpPost("attemption")]
        public async Task<ApiResponse> JoinClub([FromBody] ClubJoinDto request)
        {
            try
            {
                ClubResponseDto response = await _clubService.JoinClub(request);
                string message = "Tham gia câu lạc bộ thành công !";
                if (!response.IsPublic)
                {
                    var requesterID = Guid.Parse("3197734d-d25d-42b1-b968-84b6ee4d33c2");
                    await _clubRequestService.CreateClubRequest(requesterID, response.ClubID);
                    message = "Yêu cầu tham gia đã được gửi. Vui lòng chờ được duyệt !";
                }
                return SuccessResponse<ClubResponseDto>.Create(response, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ErrorResponse.Create("Đã xảy ra lỗi trong quá trình gửi yêu cầu !", "Er01");
            }
        }

        //[HttpPost("{id}/courses")]
        //public async Task<IActionResult> GetClubOfCourse(Guid id)
        //{

        //}

        [HttpGet("{id}/club-requests")]
        public async Task<ApiResponse> GetClubRequestsByClubID(Guid id)
        {
            try
            {
                return SuccessResponse<IEnumerable<ClubRequestResponseDto>>.Create(
                    await _clubRequestService.GetClubRequestsByID(id), "Lấy danh sách yêu cầu thành công !");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ErrorResponse.Create("Lấy danh sách không thành công !", "ER05");
            }
        }

        [HttpGet("{clubID}/participations")]
        public async Task<ApiResponse> GetClubParticipation(Guid clubID, [FromQuery] ParticipationSearchRequest searchRequest)
        {
            string message = "Lấy danh sách thành viên thành công !";
            try
            {
                return SuccessResponse<PaginationResult<UserResponse>>.Create(
                    await _clubService.GetClubParcitipations(clubID, searchRequest), message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ErrorResponse.Create(message, "Er01");
            }
        }

        [HttpGet("users")]
        public async Task<ApiResponse> GetClubsByCurrentUserID()
        {
            string message = "Lấy danh sách thành viên thành công !";
            try
            {
                return SuccessResponse<IEnumerable<ClubResponseDto>>.Create(
                    await _clubService.GetClubsByCurrentUsersID(), message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ErrorResponse.Create(message, "Er01");
            }
        }
    }
}
