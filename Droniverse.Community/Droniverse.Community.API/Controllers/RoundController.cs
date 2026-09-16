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
    /// API quản lý Round (Vòng thi)
    /// </summary>
    [ApiController]
    [Route("community/rounds")]
    [Authorize]
    public class RoundController : ControllerBase
    {
        private readonly IRoundService _roundService;
        private readonly IUserRoundService _userRoundService;

        public RoundController(IRoundService roundService, IUserRoundService userRoundService)
        {
            _roundService = roundService;
            _userRoundService = userRoundService;
        }

        /// <summary>
        /// Tạo vòng thi mới
        /// </summary>
        /// <param name="request">Thông tin vòng thi</param>
        /// <remarks>
        /// **Validation Rules:**
        /// 
        /// 1. **Thời gian Round phải nằm trong thời gian Competition**
        ///    - StartTime và EndTime phải nằm trong khoảng [Competition.StartDate, Competition.EndDate]
        /// 
        /// 2. **StartTime phải trước EndTime**
        /// 
        /// 3. **RoundNumber không được trùng**
        ///    - Mỗi competition chỉ có 1 round với mỗi số thứ tự
        /// 
        /// 4. **Thời gian không được overlap với round khác**
        ///    - Không có 2 round nào diễn ra cùng lúc
        /// 
        /// 5. **Lab không được trùng**
        ///    - Mỗi Lab chỉ được sử dụng 1 lần trong cùng competition
        /// 
        /// 6. **Lab phải tồn tại trong Academy system**
        ///    - Hệ thống sẽ kiểm tra với Academy Microservice
        /// 
        /// **Example Competition Info:**
        /// - CompetitionID: `f5364a01-da2f-4543-a850-3cf49d14e174`
        /// - Competition Period: `2024-01-10 → 2024-01-20`
        /// </remarks>
        /// <returns>201 Created - Tạo vòng thi thành công</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(RoundCreateDto), typeof(RoundCreateExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> CreateRound([FromBody] RoundCreateDto request)
        {
            var round = await _roundService.CreateRound(request);
            return SuccessResponse<RoundResponseDto>.Create(
                round,
                "Tạo vòng thi thành công!"
            );
        }

        /// <summary>
        /// Cập nhật thông tin vòng thi
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <remarks>
        /// **Validation Rules:**
        /// 
        /// 1. **Chỉ update được khi Round đang ở trạng thái Pending (0)**
        /// 
        /// 2. **Các validation giống CreateRound**
        ///    - Thời gian trong khoảng Competition
        ///    - StartTime &lt; EndTime
        ///    - Không overlap với round khác (trừ round hiện tại)
        ///    - RoundNumber không trùng với round khác (trừ round hiện tại)
        ///    - Lab không trùng với round khác (trừ round hiện tại)
        ///    - Lab tồn tại (chỉ check nếu LabID thay đổi)
        /// </remarks>
        /// <returns>200 OK - Cập nhật thành công</returns>
        [HttpPut("{roundId}")]
        [ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(RoundUpdateDto), typeof(RoundUpdateExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> UpdateRound(Guid roundId, [FromBody] RoundUpdateDto request)
        {
            var round = await _roundService.UpdateRound(roundId, request);
            return SuccessResponse<RoundResponseDto>.Create(
                round,
                "Cập nhật vòng thi thành công!"
            );
        }

        [HttpPut("{roundId:guid}/non-logic")]
        [ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(UpdateRoundNoLogicRequest), typeof(RoundUpdateNoLogicExample))]
        public async Task<ApiResponse> UpdateRoundNoLogic(Guid roundId, [FromBody] UpdateRoundNoLogicRequest request)
        {
            var round = await _roundService.UpdateRoundNoLogic(roundId, request);
            return SuccessResponse<RoundResponseDto>.Create(
                round,
                "Cập nhật thời gian vòng thi (không kiểm tra logic) thành công!"
            );
        }

        /// <summary>
        /// Lấy thông tin vòng thi theo ID
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <returns>200 OK - Trả về thông tin vòng thi</returns>
        [HttpGet("{roundId}")]
        [ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetRoundById(Guid roundId)
        {
            var round = await _roundService.GetRoundById(roundId);
            return SuccessResponse<RoundResponseDto>.Create(
                round,
                "Lấy thông tin vòng thi thành công!"
            );
        }

        ///// <summary>
        ///// Bắt đầu vòng thi
        ///// </summary>
        ///// <param name="roundId">ID của vòng thi</param>
        ///// <returns>200 OK - Bắt đầu vòng thi thành công</returns>
        //[HttpPut("{roundId}/start")]
        //[ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[Authorize(Roles = Roles.AdminOrManagerRoles)]
        //public async Task<ApiResponse> StartRound(Guid roundId)
        //{
        //    var round = await _roundService.StartRound(roundId);
        //    return SuccessResponse<RoundResponseDto>.Create(
        //        round,
        //        "Bắt đầu vòng thi thành công!"
        //    );
        //}

        ///// <summary>
        ///// Kết thúc vòng thi
        ///// </summary>
        ///// <param name="roundId">ID của vòng thi</param>
        ///// <returns>200 OK - Kết thúc vòng thi thành công</returns>
        //[HttpPut("{roundId}/finish")]
        //[ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[Authorize(Roles = Roles.AdminOrManagerRoles)]
        //public async Task<ApiResponse> FinishRound(Guid roundId)
        //{
        //    var round = await _roundService.FinishRound(roundId);
        //    return SuccessResponse<RoundResponseDto>.Create(
        //        round,
        //        "Kết thúc vòng thi thành công!"
        //    );
        //}

        /// <summary>
        /// Lấy bảng xếp hạng của vòng thi
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <remarks>
        /// Rule sắp xếp bảng xếp hạng:
        /// 1. Sắp xếp theo Point (điểm) giảm dần.
        /// 2. Nếu điểm bằng nhau, ưu tiên ExecutionTime nhỏ hơn (thời gian thực hiện ít hơn).
        /// 3. Nếu cả điểm và thời gian bằng nhau, ưu tiên SubmittedAt sớm hơn (nộp sớm hơn).
        /// </remarks>
        /// <returns>200 OK - Trả về bảng xếp hạng</returns>
        [HttpGet("{roundId}/leaderboard")]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<RoundLeaderBoardResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetRoundLeaderboard(Guid roundId, [FromQuery] RoundLeaderboardSearchRequest request)
        {
            var leaderboard = await _roundService.GetRoundLeaderboard(roundId, request);
            return SuccessResponse<PaginationResult<RoundLeaderBoardResponse>>.Create(
                leaderboard,
                "Lấy bảng xếp hạng vòng thi thành công!"
            );
        }

        /// <summary>
        /// Tham gia vòng thi cho người dùng hiện tại.
        /// </summary>
        /// <remarks>
        /// - Chỉ cho phép tham gia khi vòng đang diễn ra, người dùng chưa tham gia trước đó,
        /// - và nếu không phải vòng đầu tiên thì phải vượt qua vòng trước (`IsPassed = true`).
        /// </remarks>
        /// <param name="roundId">ID của vòng thi</param>
        /// <returns>200 OK - Tham gia thành công</returns>
        [HttpPost("{roundId}/join")]
        [ProducesResponseType(typeof(SuccessResponse<RoundJoinResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> JoinRound(Guid roundId)
        {
            var result = await _roundService.JoinRound(roundId);
            return SuccessResponse<RoundJoinResponse>.Create(
                result,
                "Tham gia vòng thi thành công!"
            );
        }

        /// <summary>
        /// Tính tổng số thí sinh trong bảng xếp hạng của vòng thi.
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <remarks>
        /// API này dùng để trigger tổng hợp leaderboard (nếu cần) và chỉ trả về tổng số lượng thí sinh
        /// có trong bảng xếp hạng, không trả danh sách chi tiết.
        /// </remarks>
        /// <returns>200 OK - Tính tổng số lượng thành công</returns>
        [HttpPost("{roundId}/leaderboard/aggregate")]
        [ProducesResponseType(typeof(SuccessResponse<RoundLeaderboardTotalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> CalculateRoundLeaderboard(Guid roundId)
        {
            var leaderboard = await _roundService.CalculateRoundLeaderboard(roundId);

            var response = new RoundLeaderboardTotalResponse
            {
                TotalParticipants = leaderboard.roundEntries.Count
            };

            return SuccessResponse<RoundLeaderboardTotalResponse>.Create(
                response,
                "Tính tổng số lượng bảng xếp hạng thành công!"
            );
        }

        /// <summary>
        /// Lấy danh sách người tham gia vòng thi theo điều kiện lọc và phân trang.
        /// </summary>
        /// <param name="roundId">ID của vòng thi</param>
        /// <param name="request">Thông tin lọc theo người dùng, thời gian tham gia, thời gian nộp bài và trạng thái</param>
        /// <returns>200 - OK: Trả về danh sách người tham gia theo phân trang</returns>
        [HttpGet("{roundId}/participants")]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<RoundParticipantsResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> GetRoundParticipants(Guid roundId, [FromQuery] RoundParticipantsSearchRequest request)
        {
            var result = await _roundService.GetRoundParticipants(roundId, request);
            return SuccessResponse<PaginationResult<RoundParticipantsResponse>>.Create(result, "Lấy danh sách tham gia của vòng thi thành công !");
        }

        /// <summary>
        /// Nộp bài cho vòng thi của người dùng hiện tại
        /// </summary>
        /// <remarks>
        /// Rule:
        /// - Người dùng phải đã tham gia vòng thi trước đó.
        /// - Chỉ nộp khi vòng thi đang diễn ra hợp lệ.
        /// - Khi nộp thành công, trạng thái `UserRound` được cập nhật thành `Completed`.
        /// - Nếu thời điểm nộp lớn hơn deadline của người dùng, `SubmittedAt` sẽ được gán bằng deadline.
        /// </remarks>
        /// <param name="roundId">ID của vòng thi</param>
        /// <param name="request">Giải pháp</param>
        /// <returns>200 OK - Submit thành công</returns>
        [HttpPost("{roundId}/submissions")]
        [ProducesResponseType(typeof(SuccessResponse<SubmitSolutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(UserRoundSubmitDto), typeof(UserRoundSubmitExample))]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> SubmitSolution(Guid roundId, [FromBody] UserRoundSubmitDto request)
        {
            var result = await _userRoundService.SubmitSolution(roundId, request);
            return SuccessResponse<SubmitSolutionResponse>.Create(
                result,
                "Submit giải pháp thành công!"
            );
        }

        /// <summary>
        /// Api dùng để cập nhật trạng thái của round
        /// </summary>
        /// <remarks>
        /// - API dùng cho CLUB_MANAGER, SYSTEM_MANAGER, ADMIN
        /// </remarks>
        /// <param name="roundId">ID của vòng thi</param>
        /// <returns></returns>
        /// 
        [Authorize(Roles = Roles.SystemRoles)]
        [HttpPatch("{roundId}/cancel")]
        public async Task<ApiResponse> UpdateRoundStatus(Guid roundId)
        {
            var result = await _roundService.UpdateRoundStatus(roundId);
            return SuccessResponse<RoundResponseDto>.Create(result, "Vòng thi được hủy thành công !");
        }
    }
}
