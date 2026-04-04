using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    /// <summary>
    /// API quản lý giải thưởng cuộc thi
    /// </summary>
    [ApiController]
    [Route("community/competition-prizes")]
    [Authorize]
    public class CompetitionPrizeController : ControllerBase
    {
        private readonly ICompetitionPrizeService _competitionPrizeService;

        public CompetitionPrizeController(ICompetitionPrizeService competitionPrizeService)
        {
            _competitionPrizeService = competitionPrizeService;
        }

        /// <summary>
        /// Tạo giải thưởng cho cuộc thi
        /// </summary>
        /// <param name="request">Thông tin giải thưởng</param>
        /// <returns>201 Created - Tạo giải thưởng thành công</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionPrizeResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(CompetitionPrizeCreateDto), typeof(CompetitionPrizeCreateExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> CreatePrize([FromBody] CompetitionPrizeCreateDto request)
        {
            var prize = await _competitionPrizeService.CreatePrize(request);
            return SuccessResponse<CompetitionPrizeResponseDto>.Create(
                prize,
                "Tạo giải thưởng thành công!"
            );
        }

        /// <summary>
        /// Cập nhật thông tin giải thưởng
        /// </summary>
        /// <param name="id">ID của giải thưởng</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>200 OK - Cập nhật thành công</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionPrizeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(CompetitionPrizeUpdateDto), typeof(CompetitionPrizeUpdateExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> UpdatePrize(Guid id, [FromBody] CompetitionPrizeUpdateDto request)
        {
            var prize = await _competitionPrizeService.UpdatePrize(id, request);
            return SuccessResponse<CompetitionPrizeResponseDto>.Create(
                prize,
                "Cập nhật giải thưởng thành công!"
            );
        }

        /// <summary>
        /// Xóa giải thưởng
        /// </summary>
        /// <param name="id">ID của giải thưởng</param>
        /// <returns>200 OK - Xóa thành công</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> DeletePrize(Guid id)
        {
            var result = await _competitionPrizeService.DeletePrize(id);
            if (!result)
                return ErrorResponse.Create("Xóa giải thưởng thất bại!", "ERR_PRIZE_DELETE");

            return SuccessResponse<string>.Create(
                null,
                "Xóa giải thưởng thành công!"
            );
        }
    }
}
