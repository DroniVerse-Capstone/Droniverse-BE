using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    /// <summary>
    /// API quản lý thông tin Câu lạc bộ (Club)
    /// Bao gồm các chức năng: lấy danh sách, lấy chi tiết, tạo mới, cập nhật,
    /// xóa, tham gia câu lạc bộ và lấy danh sách khóa học thuộc câu lạc bộ.
    /// </summary>
    [Route("community/club-policies")]
    [ApiController]
    [Authorize]
    public class ClubPolicyController : ControllerBase
    {
        private readonly IClubPolicyService _clubPolicyService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ICompetitionService _competitionService;

        public ClubPolicyController(IClubPolicyService clubPolicyService, ICloudinaryService cloudinaryService, ICompetitionService competitionService)
        {
            _clubPolicyService = clubPolicyService;
            _cloudinaryService = cloudinaryService;
            _competitionService = competitionService;
        }
        /// <summary>
        /// Lấy toàn bộ danh sách policy của câu lạc bộ
        /// </summary>
        /// <remarks>
        /// API trả về danh sách tất cả các policy hiện có trong hệ thống.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về danh sách ClubPolicyResponseDto
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAllClubPolicies([FromQuery] GetAllClubPoliciesSearchRequest request)
        {
            var result = await _clubPolicyService.GetAllClubPolicies(request);
            return Ok(result);
        }

        /// <summary>
        /// Tạo mới một policy cho câu lạc bộ.
        /// </summary>
        /// <param name="clubPolicyRequest">Thông tin dữ liệu dùng để tạo policy mới cho câu lạc bộ. (API này dành cho Manager của hệ thống)</param>
        /// <remarks>
        /// Sau khi tạo thành công hệ thống sẽ trả về HTTP 201 và dữ liệu chi tiết của policy.
        /// </remarks>
        /// <returns>
        /// 201 Created - Tạo thành công và trả về dữ liệu <see cref="ClubPolicyResponseDto"/>.
        /// 400 BadRequest - Nếu dữ liệu đầu vào không hợp lệ.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessResponse<ClubPolicyResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(ClubPolicyCreateDto), typeof(ClubPolicyCreateRequestExample))]
        [Authorize(Roles = Roles.SystemRoles)]
        [HttpPost]
        public async Task<IActionResult> CreateClubPolicy([FromBody] ClubPolicyCreateDto request)
        {
            var result = await _clubPolicyService.CreateClubPolicyAsync(request);
            return Ok(result);
        }
    }
}