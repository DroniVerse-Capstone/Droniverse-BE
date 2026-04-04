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

        public RoundController(IRoundService roundService)
        {
            _roundService = roundService;
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
        /// <param name="id">ID của vòng thi</param>
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
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(RoundUpdateDto), typeof(RoundUpdateExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> UpdateRound(Guid id, [FromBody] RoundUpdateDto request)
        {
            var round = await _roundService.UpdateRound(id, request);
            return SuccessResponse<RoundResponseDto>.Create(
                round,
                "Cập nhật vòng thi thành công!"
            );
        }

        /// <summary>
        /// Lấy thông tin vòng thi theo ID
        /// </summary>
        /// <param name="id">ID của vòng thi</param>
        /// <returns>200 OK - Trả về thông tin vòng thi</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetRoundById(Guid id)
        {
            var round = await _roundService.GetRoundById(id);
            return SuccessResponse<RoundResponseDto>.Create(
                round,
                "Lấy thông tin vòng thi thành công!"
            );
        }

        /// <summary>
        /// Bắt đầu vòng thi
        /// </summary>
        /// <param name="id">ID của vòng thi</param>
        /// <returns>200 OK - Bắt đầu vòng thi thành công</returns>
        [HttpPut("{id}/start")]
        [ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> StartRound(Guid id)
        {
            var round = await _roundService.StartRound(id);
            return SuccessResponse<RoundResponseDto>.Create(
                round,
                "Bắt đầu vòng thi thành công!"
            );
        }

        /// <summary>
        /// Kết thúc vòng thi
        /// </summary>
        /// <param name="id">ID của vòng thi</param>
        /// <returns>200 OK - Kết thúc vòng thi thành công</returns>
        [HttpPut("{id}/finish")]
        [ProducesResponseType(typeof(SuccessResponse<RoundResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> FinishRound(Guid id)
        {
            var round = await _roundService.FinishRound(id);
            return SuccessResponse<RoundResponseDto>.Create(
                round,
                "Kết thúc vòng thi thành công!"
            );
        }

        /// <summary>
        /// Lấy bảng xếp hạng của vòng thi
        /// </summary>
        /// <param name="id">ID của vòng thi</param>
        /// <remarks>
        /// Rule sắp xếp bảng xếp hạng:
        /// 1. Sắp xếp theo Point (điểm) giảm dần.
        /// 2. Nếu điểm bằng nhau, ưu tiên ExecutionTime nhỏ hơn (thời gian thực hiện ít hơn).
        /// 3. Nếu cả điểm và thời gian bằng nhau, ưu tiên SubmittedAt sớm hơn (nộp sớm hơn).
        /// </remarks>
        /// <returns>200 OK - Trả về bảng xếp hạng</returns>
        [HttpGet("{id}/leaderboard")]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<RoundLeaderBoardResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetRoundLeaderboard(Guid id, [FromQuery] RoundLeaderboardSearchRequest request)
        {
            var leaderboard = await _roundService.GetRoundLeaderboard(id, request);
            return SuccessResponse<PaginationResult<RoundLeaderBoardResponse>>.Create(
                leaderboard,
                "Lấy bảng xếp hạng vòng thi thành công!"
            );
        }

        /// <summary>
        /// Tham gia vòng thi
        /// </summary>
        /// <param name="id">ID của vòng thi</param>
        /// <param name="request">Thông tin đăng ký tham gia</param>
        /// <returns>200 OK - Tham gia thành công</returns>
        //[HttpPost("{id}/join")]
        //[ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ApiResponse> JoinRound(Guid id)
        //{
        //    await _roundService.JoinRound(id, request);
        //    return SuccessResponse<string>.Create(
        //        "Tham gia vòng thi thành công!"
        //    );
        //}
    }
}
