using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Extensions;
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
    /// API quản lý kết quả vòng thi của người dùng
    /// </summary>
    [ApiController]
    [Route("community/user-rounds")]
    [Authorize]
    public class UserRoundController : ControllerBase
    {
        private readonly IUserRoundService _userRoundService;

        public UserRoundController(IUserRoundService userRoundService)
        {
            _userRoundService = userRoundService;
        }

        /// <summary>
        /// Lấy danh sách vòng thi mà người dùng hiện tại đã tham gia theo bộ lọc và phân trang.
        /// </summary>
        /// <param name="request">Điều kiện lọc trạng thái vòng thi, trạng thái bài thi và kết quả qua vòng</param>
        /// <returns>200 OK - Trả về kết quả</returns>
        [HttpGet("users/me/rounds")]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<MyRoundsResultResponse>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> GetUserRoundResult([FromQuery] MyRoundSearchRequest request)
        {
            var result = await _userRoundService.GetMyUserRound(request);
            return SuccessResponse<PaginationResult<IEnumerable<MyRoundsResultResponse>>>.Create(
                result,
                "Lấy kết quả vòng thi thành công!"
            );
        }

        /// <summary>
        /// Lấy kết quả vòng thi của người dùng hiện tại
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <returns>200 OK - Trả về kết quả</returns>
        [HttpGet("users/me/rounds/{roundId}")]
        [ProducesResponseType(typeof(SuccessResponse<UserRoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> GetUserRoundResult(Guid roundId)
        {
            var result = await _userRoundService.GetUserRoundResult(roundId);
            return SuccessResponse<UserRoundResponseDto>.Create(
                result,
                "Lấy kết quả vòng thi thành công!"
            );
        }


        /// <summary>
        /// Lấy danh sách kết quả tất cả thí sinh của vòng thi theo bộ lọc và phân trang.
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <param name="request">Điều kiện lọc trạng thái bài thi, sắp xếp và phân trang</param>
        /// <returns>200 OK - Trả về danh sách kết quả</returns>
        [HttpGet("users/rounds/{roundId}")]
        [ProducesResponseType(typeof(SuccessResponse<RoundResultsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ApiResponse> GetRoundResults(Guid roundId, [FromQuery] RoundResultAllParicipations request)
        {
            var results = await _userRoundService.GetRoundResults(roundId, request);
            return SuccessResponse<RoundResultsDto>.Create(
                results,
                "Lấy danh sách kết quả vòng thi thành công!"
            );
        }

        /// <summary>
        /// Lấy kết quả vòng thi của một người dùng theo mã người dùng và mã vòng thi.
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="roundId">ID của vòng thi</param>
        /// <returns>200 OK - Trả về kết quả chi tiết</returns>
        [HttpGet("users/{userId}/rounds/{roundId}")]
        [ProducesResponseType(typeof(SuccessResponse<UserRoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ApiResponse> GetRoundResultByUser(Guid userId, Guid roundId)
        {
            var results = await _userRoundService.GetRoundResultByUser(userId, roundId);
            return SuccessResponse<UserRoundResponseDto>.Create(
                results,
                "Lấy kết quả vòng thi theo người dùng thành công!"
            );
        }
    }
}
