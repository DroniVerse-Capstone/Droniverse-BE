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
    [Route("community/clubs")]
    [ApiController]
    [Authorize]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _clubService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ICompetitionService _competitionService;

        public ClubController(IClubService clubService, ICloudinaryService cloudinaryService, ICompetitionService competitionService)
        {
            _clubService = clubService;
            _cloudinaryService = cloudinaryService;
            _competitionService = competitionService;
        }

        [HttpGet("/{clubId}/get-drone-from-club")]
        public async Task<ApiResponse> GetDroneFromClub(Guid clubId)
        {
            Guid result = await _clubService.GetDroneFromClub(clubId);
            return SuccessResponse<Guid>.Create(result, "Lấy thông tin drone của club thành công!");
        }

        /// <summary>
        /// Lấy toàn bộ danh sách câu lạc bộ
        /// </summary>
        /// <remarks>
        /// API trả về danh sách tất cả các club hiện có trong hệ thống.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về danh sách ClubResponseDto
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<ClubResponseDto>>>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetAllCLubs([FromQuery] GetAllClubsSearchRequest request)
        {

            PaginationResult<IEnumerable<ClubResponseDto>> clubs = await _clubService.GetAllClubs(request);
            return SuccessResponse<PaginationResult<IEnumerable<ClubResponseDto>>>
                .Create(clubs, "Lấy danh sách câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một câu lạc bộ theo ID
        /// </summary>
        /// <param name="clubId">test : d1822ac3-00ac-46db-9b74-3b4df9621765</param>
        /// <remarks>
        /// Nếu không tìm thấy club theo ID truyền vào, service có thể throw exception.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về thông tin chi tiết ClubResponseDto  
        /// 404 NotFound - Nếu không tồn tại club
        /// </returns>
        [HttpGet("{clubId}")]
        [ProducesResponseType(typeof(SuccessResponse<ClubResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetClubById(
            [FromRoute]
            Guid clubId)
        {

            ClubResponseDto club = await _clubService.GetClubById(clubId);
            return SuccessResponse<ClubResponseDto>
                .Create(club, $"Lấy thông tin câu lạc bộ với ID [{clubId}] thành công!");
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một câu lạc bộ theo Club Code
        /// </summary>
        /// <param name="clubCode">Test : X7J968, A6DD3J</param>
        /// <remarks>
        /// Nếu không tìm thấy club theo club code truyền vào, service có thể throw exception.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về thông tin chi tiết ClubResponseDto  
        /// 404 NotFound - Nếu không tồn tại club
        /// </returns>
        [HttpGet("code/{clubCode}")]
        [ProducesResponseType(typeof(SuccessResponse<ClubResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> GetClubByClubCode(
            [FromRoute]
            string clubCode)
        {

            ClubResponseDto club = await _clubService.GetClubByClubCode(clubCode);
            return SuccessResponse<ClubResponseDto>.Create(club, $"Lấy thông tin câu lạc bộ với club code [{clubCode}] thành công!");
        }

        /// <summary>
        /// Lấy danh sách khóa học HOT của một câu lạc bộ theo phân trang.
        /// </summary>
        /// <param name="clubId">ID của câu lạc bộ</param>
        /// <param name="searchRequest">Thông tin phân trang</param>
        /// <returns>
        /// 200 OK - Trả về danh sách khóa học HOT theo <see cref="PaginationResult{T}"/>
        /// 404 NotFound - Nếu không tồn tại câu lạc bộ
        /// </returns>
        //[HttpGet("{clubId}/courses/hot")]
        //[ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<CourseBulkResponseDTO>>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ApiResponse> GetHotCoursesByClub(Guid clubId, [FromQuery] HotCoursesSearchRequest searchRequest)
        //{
        //    var courses = await _clubService.GetHotCoursesByClub(clubId, searchRequest);
        //    return SuccessResponse<PaginationResult<IEnumerable<CourseBulkResponseDTO>>>.Create(
        //        courses,
        //        "Lấy danh sách khóa học HOT của câu lạc bộ thành công!");
        //}

        ///// <summary>
        ///// Tạo mới một câu lạc bộ.
        ///// </summary>
        ///// <param name="clubRequest">Thông tin dữ liệu dùng để tạo câu lạc bộ mới. (API này dành cho Manager của hệ thống)</param>
        ///// <remarks>
        ///// Sau khi tạo thành công hệ thống sẽ trả về HTTP 201 và dữ liệu chi tiết của câu lạc bộ.
        ///// </remarks>
        ///// <returns>
        ///// 201 Created - Tạo thành công và trả về dữ liệu <see cref="ClubResponseDto"/>.
        ///// 400 BadRequest - Nếu dữ liệu đầu vào không hợp lệ.
        ///// </returns>
        //[HttpPost]
        //[ProducesResponseType(typeof(SuccessResponse<ClubResponseDto>), StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[SwaggerRequestExample(typeof(ClubAttemptRequestCreateDto), typeof(ClubCreateMultipleExample))]
        //[Authorize(Roles = Roles.SystemRoles)]
        //public async Task<ApiResponse> CreateClub([FromBody] ClubCreateDto clubRequest)
        //{
        //    ClubResponseDto createdClub = await _clubService.CreateClub(clubRequest);

        //    return SuccessResponse<ClubResponseDto>
        //        .Create(createdClub, "Tạo câu lạc bộ thành công!");
        //}

        /// <summary>
        /// Gửi yêu cầu tham gia câu lạc bộ
        /// </summary>
        /// <param name="request">Thông tin yêu cầu tham gia club</param>
        /// <remarks>
        /// Nếu club là public có thể được tham gia trực tiếp.  
        /// Nếu club là private có thể tạo một yêu cầu chờ duyệt.
        /// </remarks>
        /// <example>
        /// A41YQ1
        /// </example>
        /// <returns>
        /// 200 OK - Trả về thông tin club sau khi xử lý tham gia
        /// </returns>
        [HttpPost("attemption")]
        [ProducesResponseType(typeof(SuccessResponse<JoinClubResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> JoinClub([FromBody] ClubJoinDto request)
        {
            string message = "Thành viên gửi yêu cầu tham gia câu lạc bộ thành công! Vui lòng đợi sự kiểm duyệt từ phía hệ thống...";
            JoinClubResponse response = await _clubService.JoinClub(request);
            return SuccessResponse<JoinClubResponse>
                .Create(response, message);
        }

        /// <summary>
        /// Cập nhật thông tin câu lạc bộ theo ID (không bao gồm status)
        /// </summary>
        /// <param name="clubId">GUID của câu lạc bộ cần cập nhật</param>
        /// <param name="clubRequest">Dữ liệu cập nhật</param>
        /// <remarks>
        /// API này chỉ cho phép cập nhật thông tin cơ bản: tên, mô tả, categories, giới hạn thành viên.
        /// Để thay đổi trạng thái (status), vui lòng sử dụng các API riêng biệt.
        /// </remarks>
        /// <returns>
        /// 200 OK - Cập nhật thành công và trả về ClubResponseDto  
        /// 400 BadRequest - Nếu dữ liệu không hợp lệ  
        /// 404 NotFound - Nếu không tồn tại club
        /// </returns>
        [HttpPut("{clubId}")]
        [ProducesResponseType(typeof(SuccessResponse<ClubResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(ClubUpdateDto), typeof(ClubUpdateExample))]
        [Authorize(Roles = Roles.ClubManager)]
        public async Task<ApiResponse> UpdateClub(Guid clubId, [FromBody] ClubUpdateDto clubRequest)
        {
            ClubResponseDto updatedClub = await _clubService.UpdateClub(clubId, clubRequest);
            return SuccessResponse<ClubResponseDto>
                .Create(updatedClub, $"Cập nhật câu lạc bộ với ID [{clubId}] thành công!");
        }

        // ===== Status Management Endpoints =====

        /// <summary>
        /// Cập nhật trạng thái câu lạc bộ - Unified Endpoint
        /// </summary>
        /// <param name="clubId">GUID của câu lạc bộ</param>
        /// <param name="request">Thông tin cập nhật trạng thái</param>
        /// <remarks>
        /// **API gộp để cập nhật mọi trạng thái của Club với phân quyền tự động**
        /// 
        /// ### Quyền hạn theo Status:
        /// 
        /// | Status | Quyền yêu cầu | Mô tả |
        /// |--------|---------------|-------|
        /// | **SUSPENDED (2)** | ADMIN, SYSTEM_MANAGER | Đình chỉ club do vi phạm |
        /// | **INACTIVE (0)** | CLUB_MANAGER (owner) | Tạm ngừng hoạt động |
        /// | **ARCHIVED (3)** | CLUB_MANAGER (owner) | Đóng hẳn club |
        /// | **ACTIVE (1)** | ADMIN, SYSTEM_MANAGER,CLUB_MANAGER (owner) | Khôi phục club |
        /// 
        /// ### Lưu ý:
        /// - **SUSPENDED** và **ARCHIVED** bắt buộc phải có `Reason`
        /// - CLUB_MANAGER chỉ có thể thao tác với club mình tạo
        /// - Không thể khôi phục club đã ARCHIVED
        /// 
        /// ### Example Request:
        /// ```json
        /// {
        ///   "status": 2,
        ///   "reason": "Vi phạm quy định về nội dung"
        /// }
        /// ```
        /// </remarks>
        /// <returns>200 OK - Cập nhật trạng thái thành công</returns>
        [HttpPut("{clubId}/status")]
        [ProducesResponseType(typeof(SuccessResponse<ClubResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)] // Require at least one of these roles
        public async Task<ApiResponse> UpdateClubStatus(
            Guid clubId,
            [FromBody] ClubUpdateStatusDto request)
        {
            var club = await _clubService.UpdateClubStatus(clubId, request);

            string statusMessage = request.Status switch
            {
                Domain.Enums.ClubStatus.SUSPENDED => "đình chỉ",
                Domain.Enums.ClubStatus.INACTIVE => "tạm ngừng hoạt động",
                Domain.Enums.ClubStatus.ARCHIVED => "đóng hẳn",
                Domain.Enums.ClubStatus.ACTIVE => "khôi phục",
                _ => "cập nhật trạng thái"
            };

            return SuccessResponse<ClubResponseDto>
                .Create(club, $"Đã {statusMessage} câu lạc bộ với ID [{clubId}] thành công!");
        }

        /// <summary>
        /// Lấy danh sách thành viên của một câu lạc bộ
        /// </summary>
        /// <param name="clubId">GUID của câu lạc bộ</param>
        /// <param name="searchRequest">Thông tin phân trang và lọc</param>
        /// <returns>
        /// 200 OK - Trả về danh sách thành viên có phân trang
        /// </returns>
        [HttpGet("{clubId}/participants")]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<GetParticipantsResponse>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetClubParticipations(Guid clubId, [FromQuery] ParticipationSearchRequest searchRequest)
        {
            var participations = await _clubService.GetClubParcitipations(clubId, searchRequest);
            return SuccessResponse<PaginationResult<IEnumerable<GetParticipantsResponse>>>.Create(participations, "Lấy danh sách thành viên câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy danh sách câu lạc bộ mà người dùng hiện tại đang tham gia hoặc quản lý
        /// </summary>
        /// <param name="status">Lọc theo trạng thái club: 0=INACTIVE, 1=ACTIVE, 2=SUSPENDED, 3=ARCHIVED. Null = lấy tất cả</param>
        /// <remarks>
        /// ### Quyền hạn:
        /// - **CLUB_MEMBER**: Lấy danh sách clubs đã tham gia (qua Participation)
        /// - **CLUB_MANAGER/ADMIN/SYSTEM_MANAGER**: Lấy danh sách clubs đã tạo/quản lý
        /// 
        /// ### Filter Status:
        /// - Không truyền `status` hoặc `status=null`: Lấy tất cả clubs (mọi trạng thái)
        /// - `status=0`: Chỉ lấy clubs INACTIVE
        /// - `status=1`: Chỉ lấy clubs ACTIVE
        /// - `status=2`: Chỉ lấy clubs SUSPENDED
        /// - `status=3`: Chỉ lấy clubs ARCHIVED
        /// 
        /// ### Ví dụ:
        /// ```
        /// GET /community/clubs/myclub              // Lấy tất cảmy-club
        /// GET /community/clubs/myclub?status=1     // Chỉ lấy ACTIVE
        /// GET /community/clubs/myclub?status=2     // Chỉ lấy SUSPENDED
        /// ```
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về danh sách club của người dùng hiện tại (đã filter theo status nếu có)
        /// </returns>
        [HttpGet("myclub")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ClubResponseDto>>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.ClubRoles)]
        public async Task<ApiResponse> GetMyClubs([FromQuery] Domain.Enums.ClubStatus? status = null)
        {
            var clubs = await _clubService.GetClubsByCurrentUsersID(status);

            string message = status.HasValue
                ? $"Lấy danh sách câu lạc bộ [{GetStatusDisplayName(status.Value)}] thành công!"
                : "Lấy danh sách câu lạc bộ đang tham gia thành công!";

            return SuccessResponse<IEnumerable<ClubResponseDto>>
                .Create(clubs, message);
        }

        /// <summary>
        /// Helper method để lấy tên hiển thị của status
        /// </summary>
        private static string GetStatusDisplayName(Domain.Enums.ClubStatus status)
        {
            return status switch
            {
                Domain.Enums.ClubStatus.ACTIVE => "đang hoạt động",
                Domain.Enums.ClubStatus.INACTIVE => "tạm ngừng",
                Domain.Enums.ClubStatus.SUSPENDED => "bị đình chỉ",
                Domain.Enums.ClubStatus.ARCHIVED => "đã đóng",
                _ => ""
            };
        }

        /// <summary>
        /// Lấy danh sách cuộc thi của một club.
        /// </summary>
        /// <param name="clubId">ID của club</param>
        /// <param name="status">
        /// Trạng thái cuộc thi. Để null để lấy tất cả.
        /// Giá trị hợp lệ:
        /// 0-DRAFT,
        /// 1-PUBLISHED,
        /// 2-REGISTRATION_OPEN,
        /// 3-REGISTRATION_CLOSED,
        /// 4-ONGOING,
        /// 5-FINISHED,
        /// 6-RESULT_PUBLISHED,
        /// 7-CANCELLED,
        /// 8-INVALID.
        /// </param>
        /// <returns>200 OK - Trả về danh sách cuộc thi</returns>
        [HttpGet("{clubId}/competitions")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<CompetitionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetCompetitionsByClub(Guid clubId, [FromQuery] CompetitionStatus? status = null)
        {
            var competitions = await _competitionService.GetCompetitionsByClub(clubId, status);
            return SuccessResponse<IEnumerable<CompetitionResponse>>.Create(
                competitions,
                "Lấy danh sách cuộc thi của club thành công!"
            );
        }

        /// <summary>
        /// Lấy danh sách cuộc thi HOT của một club.
        /// </summary>
        /// <param name="clubId">ID của club</param>
        /// <param name="searchRequest">Thông tin phân trang</param>
        /// <remarks>
        /// Quy tắc HOT đơn giản:
        /// - Ưu tiên competition đang hoạt động (`PUBLISHED`, `REGISTRATION_OPEN`, `REGISTRATION_CLOSED`, `ONGOING`)
        /// - Tính điểm từ độ phổ biến (participants), độ gần thời gian hiện tại (recency), trạng thái và activity gần đây
        /// - Sắp xếp theo điểm HOT giảm dần
        /// </remarks>
        /// <returns>200 OK - Trả về danh sách competition HOT có phân trang</returns>
        [HttpGet("{clubId}/competitions/hot")]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<CompetitionResponse>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetHotCompetitionsByClub(Guid clubId, [FromQuery] HotCompetitionSearchRequest searchRequest)
        {
            var competitions = await _competitionService.GetHotCompetitionsByClub(clubId, searchRequest);
            return SuccessResponse<PaginationResult<IEnumerable<CompetitionResponse>>>.Create(
                competitions,
                "Lấy danh sách cuộc thi HOT của club thành công!"
            );
        }

        //[HttpGet("{clubId:guid}/codes")]
        //[Authorize(Roles = Roles.SystemRoles)]
        //public async Task<ApiResponse> GetCodesByClub(Guid clubId, [FromQuery] GetAllCodesByClubSearchRequest request)
        //{
        //    var result = await _clubService.GetAll
        //}

        //[HttpPost("{clubId}/codes/generate")]
        //[Authorize(Roles = Roles.SystemRoles)]
        //public async Task<ApiResponse> GenerateCodesByManager(Guid clubId, [FromBody] CreateCodesRequestDTO request)
        //{
        //    var result = await _clubService.GenerateCodesByManager(clubId, request);
        //    return SuccessResponse<CreateCodesResponse>.Create(result, $"Khởi tạo {result.CreatedCode} mã thành công");
        //}

        /// <summary>
        /// Lấy nhanh thông tin cơ bản của nhiều câu lạc bộ theo danh sách ID.
        /// </summary>
        /// <param name="request">Danh sách ID câu lạc bộ cần lấy thông tin.</param>
        /// <remarks>
        /// Tối ưu hiệu năng:
        /// - Tự động loại bỏ ID trùng và ID rỗng.
        /// - Chỉ truy vấn các trường cần thiết để trả về dữ liệu nhẹ.
        /// - Trả kết quả theo đúng thứ tự ID đầu vào (sau khi loại trùng).
        /// </remarks>
        /// <returns>Danh sách thông tin rút gọn của câu lạc bộ.</returns>
        [HttpPost("clubIds/bulk")]
        [ProducesResponseType(typeof(IEnumerable<SimpleClubResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClubInfo([FromBody] GetClubSimpleInfoRequest request)
        {
            if (request.ClubIds == null || request.ClubIds.Count == 0)
                throw new ArgumentException("Danh sách ID câu lạc bộ không được để trống.");

            var result = await _clubService.GetClubInfoBulk(request);

            return Ok(result);
        }

        /// <summary>
        /// API để call chéo service 
        /// </summary>
        /// <param name="clubId">ID của câu lạc bộ</param>
        /// <param name="request">query param</param>
        /// <returns>200 : ok</returns>
        [HttpGet("{clubId:guid}/participantIds")]
        public async Task<IActionResult> GetClubParticipantResponse(Guid clubId, [FromQuery] GetClubParticipantIdsRequest request)
        {
            var result = await _clubService.GetClubParticipantIds(clubId, request);
            return Ok(result);
        }

        /// <summary>
        /// Api call chéo service để check participant
        /// </summary>
        /// <param name="clubId">ID của câu lạc bộ</param>
        /// <param name="userId">ID của user</param>
        /// <param name="status">Trạng thái của người tham gia</param>
        /// <returns></returns>
        [HttpGet("{clubId:guid}/participants/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckParticipantByClub(
            Guid clubId,
            Guid userId,
            [FromQuery] ParticipationStatus status = ParticipationStatus.ACTIVE)
        {
            var result = await _clubService.CheckParticipant(clubId, userId, status);
            return result ? Ok() : NotFound();
        }
    }
}