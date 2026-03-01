using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Extensions;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
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
        public async Task<IActionResult> JoinClub([FromBody] ClubJoinDto request)
        {
            ClubResponseDto response = await _clubService.JoinClub(request);
            return Ok(response);
        }

        [HttpGet("{id}/courses")]
        public async Task<IActionResult> GetClubCourses(Guid id)
        {
            try
            {
                var courses = await _clubService.GetClubCourses(id);
                return Ok(courses);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
