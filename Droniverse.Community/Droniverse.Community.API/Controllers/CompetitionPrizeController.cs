using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    /// <summary>
    /// API quản lý giải thưởng cuộc thi
    /// </summary>
    [ApiController]
    [Route("community/competition-prizes")]
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

        /// <summary>
        /// Lấy danh sách giải thưởng theo cuộc thi
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <returns>200 OK - Trả về danh sách giải thưởng</returns>
        [HttpGet("competition/{competitionId}")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<CompetitionPrizeResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetPrizesByCompetition(Guid competitionId)
        {
            var prizes = await _competitionPrizeService.GetPrizesByCompetition(competitionId);
            return SuccessResponse<IEnumerable<CompetitionPrizeResponseDto>>.Create(
                prizes,
                "Lấy danh sách giải thưởng thành công!"
            );
        }
    }
}
