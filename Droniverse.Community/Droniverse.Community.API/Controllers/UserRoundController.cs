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
    /// API quản lý kết quả vòng thi của người dùng
    /// </summary>
    [ApiController]
    [Route("community/user-rounds")]
    public class UserRoundController : ControllerBase
    {
        private readonly IUserRoundService _userRoundService;

        public UserRoundController(IUserRoundService userRoundService)
        {
            _userRoundService = userRoundService;
        }

        /// <summary>
        /// Submit giải pháp cho vòng thi
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <param name="request">Giải pháp</param>
        /// <returns>200 OK - Submit thành công</returns>
        [HttpPost("{roundId}/submit")]
        [ProducesResponseType(typeof(SuccessResponse<UserRoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(UserRoundSubmitDto), typeof(UserRoundSubmitExample))]
        public async Task<ApiResponse> SubmitSolution(Guid roundId, [FromBody] UserRoundSubmitDto request)
        {
            var result = await _userRoundService.SubmitSolution(roundId, request);
            return SuccessResponse<UserRoundResponseDto>.Create(
                result,
                "Submit giải pháp thành công!"
            );
        }

        /// <summary>
        /// Lấy kết quả vòng thi của người dùng hiện tại
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <returns>200 OK - Trả về kết quả</returns>
        [HttpGet("{roundId}/my-result")]
        [ProducesResponseType(typeof(SuccessResponse<UserRoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetUserRoundResult(Guid roundId)
        {
            var result = await _userRoundService.GetUserRoundResult(roundId);
            return SuccessResponse<UserRoundResponseDto>.Create(
                result,
                "Lấy kết quả vòng thi thành công!"
            );
        }

        /// <summary>
        /// Lấy tất cả kết quả của vòng thi
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <returns>200 OK - Trả về danh sách kết quả</returns>
        [HttpGet("{roundId}/results")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<UserRoundResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetAllRoundResults(Guid roundId)
        {
            var results = await _userRoundService.GetAllRoundResults(roundId);
            return SuccessResponse<IEnumerable<UserRoundResponseDto>>.Create(
                results,
                "Lấy danh sách kết quả vòng thi thành công!"
            );
        }
    }
}
