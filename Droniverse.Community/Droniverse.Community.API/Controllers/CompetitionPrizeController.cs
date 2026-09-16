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
        /// Cập nhật thông tin giải thưởng
        /// </summary>
        /// <param name="competitionPrizeId">ID của giải thưởng</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>200 OK - Cập nhật thành công</returns>
        [HttpPut("{competitionPrizeId}")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionPrizeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(CompetitionPrizeUpdateDto), typeof(CompetitionPrizeUpdateExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> UpdatePrize(Guid competitionPrizeId, [FromBody] CompetitionPrizeUpdateDto request)
        {
            var prize = await _competitionPrizeService.UpdatePrize(competitionPrizeId, request);
            return SuccessResponse<CompetitionPrizeResponseDto>.Create(
                prize,
                "Cập nhật giải thưởng thành công!"
            );
        }

        /// <summary>
        /// Xóa giải thưởng
        /// </summary>
        /// <param name="competitionPrizeId">ID của giải thưởng</param>
        /// <returns>200 OK - Xóa thành công</returns>
        [HttpDelete("{competitionPrizeId}")]
        [ProducesResponseType(typeof(SuccessResponse<DeletePrizeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> DeletePrize(Guid competitionPrizeId)
        {
            var result = await _competitionPrizeService.DeletePrize(competitionPrizeId);

            return SuccessResponse<DeletePrizeResponse>.Create(
                result,
                "Xóa giải thưởng thành công!"
            );
        }
    }
}
