using CloudinaryDotNet.Actions;
using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    /// <summary>
    /// API quản lý cuộc thi (Competition).
    /// Vòng đời trạng thái hiện tại:
    /// DRAFT(0) → PUBLISHED(1) → REGISTRATION_OPEN(2) → REGISTRATION_CLOSED(3) → ONGOING(4) → FINISHED(5) → RESULT_PUBLISHED(6).
    /// Trạng thái kết thúc đặc biệt: CANCELLED(7), INVALID(8).
    /// </summary>
    [ApiController]
    [Route("community/competitions")]
    [Authorize]
    public class CompetitionController : ControllerBase
    {
        private readonly ICompetitionService _competitionService;
        private readonly ICompetitionCertificateService _competitionCertificateService;

        public CompetitionController(
            ICompetitionService competitionService,
            ICompetitionCertificateService competitionCertificateService)
        {
            _competitionService = competitionService;
            _competitionCertificateService = competitionCertificateService;
        }

        /// <summary>
        /// Tạo cuộc thi mới (khởi tạo ở trạng thái DRAFT).
        /// </summary>
        /// <param name="request">Thông tin cuộc thi</param>
        /// <returns>201 Created - Tạo cuộc thi thành công</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(CompetitionCreationRequest), typeof(CompetitionCreationRequestExample))]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> CreateCompetition([FromBody] CompetitionCreationRequest request)
        {
            var competition = await _competitionService.CreateCompetition(request);
            return SuccessResponse<CompetitionResponse>.Create(
                competition,
                "Tạo cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Cập nhật thông tin cuộc thi.
        /// </summary>
        /// <param name="id">ID của cuộc thi</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <remarks>
        /// **Quy tắc cập nhật theo domain:**
        /// **Nên dùng các competition đã tạo với thời gian cố định.
        /// 1. **DRAFT**:
        ///    - Cho phép cập nhật đầy đủ thông tin timeline: `VisibleAt`, `RegistrationStartDate`, `RegistrationEndDate`, `StartDate`, `EndDate`
        ///    - Validate: `RegistrationStartDate &lt; RegistrationEndDate`, `StartDate &lt; EndDate`
        ///    - Validate timeline: `VisibleAt ≤ RegistrationStartDate`, `RegistrationEndDate ≤ StartDate`
        ///
        /// 2. **PUBLISHED**:
        ///    - Chỉ cập nhật thông tin nội dung: `NameVN`, `NameEN`, `DescriptionVN`, `DescriptionEN`
        ///    - Có thể cập nhật `MaxParticipants` nhưng không được nhỏ hơn số đã đăng ký
        ///    - `RuleContent` chỉ được đổi khi chưa có người đăng ký
        ///
        /// 3. **Các trạng thái khác** (`REGISTRATION_OPEN`, `REGISTRATION_CLOSED`, `ONGOING`, `FINISHED`, `RESULT_PUBLISHED`, `CANCELLED`, `INVALID`):
        ///    - Không được cập nhật bằng luồng này.
        /// </remarks>
        /// <returns>200 OK - Cập nhật thành công</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(CompetitionUpdateDto), typeof(CompetitionUpdateExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> UpdateCompetition(Guid id, [FromBody] CompetitionUpdateDto request)
        {
            var competition = await _competitionService.UpdateCompetition(id, request);
            return SuccessResponse<CompetitionResponse>.Create(
                competition,
                "Cập nhật cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Xóa cuộc thi.
        /// </summary>
        /// <param name="id">ID của cuộc thi</param>
        /// <remarks>
        /// **Quy tắc xóa:**
        ///
        /// - Chỉ cho phép xóa cuộc thi ở trạng thái `DRAFT`
        /// - Không thể xóa cuộc thi đã đi vào các giai đoạn vận hành/đã có dữ liệu phát sinh
        /// </remarks>
        /// <returns>200 OK - Xóa thành công</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> DeleteCompetition(Guid id)
        {
            var result = await _competitionService.DeleteCompetition(id);
            if (!result)
                return ErrorResponse.Create("Xóa cuộc thi thất bại!", "ERR_DELETE_FAILED");

            return SuccessResponse<string>.Create(
                null,
                "Xóa cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Lấy thông tin chi tiết cuộc thi theo ID.
        /// </summary>
        /// <param name="id">ID của cuộc thi</param>
        /// <returns>200 OK - Trả về thông tin cuộc thi</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetCompetitionById(Guid id)
        {
            var competition = await _competitionService.GetCompetitionById(id);
            return SuccessResponse<CompetitionResponse>.Create(
                competition,
                "Lấy thông tin cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Lấy danh sách cuộc thi theo điều kiện lọc.
        /// </summary>
        /// <remarks>
        /// API Chỉ dùng cho role `ADMIN` và `SYSTEM_MANAGER`
        /// </remarks>
        /// <param name="searchRequest">Điều kiện tìm kiếm (competition name, timeline, status)</param>
        /// <returns>200 OK - Trả về danh sách cuộc thi có phân trang</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<CompetitionResponse>>>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetAllCompetitionWithCondition([FromQuery] CompetitionSearchRequest searchRequest)
        {
            var competitions = await _competitionService.GetAllCompetitionsWithCondition(searchRequest);
            return SuccessResponse<PaginationResult<IEnumerable<CompetitionResponse>>>.Create(
                competitions,
                "Lấy danh sách cuộc thi thành công!"
            );
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
        [HttpGet("club/{clubId}")]
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
        [HttpGet("club/{clubId}/hot")]
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

        /// <summary>
        /// Đăng ký tham gia cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <remarks>
        /// Chỉ hợp lệ khi cuộc thi ở trạng thái `REGISTRATION_OPEN` và thời điểm hiện tại nằm trong khoảng đăng ký.
        /// </remarks>
        /// <returns>200 OK - Đăng ký thành công</returns>
        [HttpPost("{competitionId}/register")]
        [ProducesResponseType(typeof(SuccessResponse<UserCompetitionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> RegisterForCompetition(Guid competitionId)
        {
            var registration = await _competitionService.RegisterForCompetition(competitionId);
            return SuccessResponse<UserCompetitionResponseDto>.Create(
                registration,
                "Đăng ký tham gia cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Rút khỏi cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <returns>200 OK - Rút khỏi cuộc thi thành công</returns>
        [HttpPost("{competitionId}/withdraw")]
        [ProducesResponseType(typeof(SuccessResponse<UserCompetitionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.ClubMember)]
        public async Task<ApiResponse> WithdrawFromCompetition(Guid competitionId)
        {
            var result = await _competitionService.WithdrawFromCompetition(competitionId);
            return SuccessResponse<UserCompetitionResponseDto>.Create(
                result,
                "Rút khỏi cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Lấy danh sách thí sinh tham gia cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <returns>200 OK - Trả về danh sách thí sinh</returns>
        [HttpGet("{competitionId}/participants")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<UserCompetitionResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetCompetitionParticipants(Guid competitionId)
        {
            var participants = await _competitionService.GetCompetitionParticipants(competitionId);
            return SuccessResponse<IEnumerable<UserCompetitionResponseDto>>.Create(
                participants,
                "Lấy danh sách thí sinh thành công!"
            );
        }

        /// <summary>
        /// Lấy bảng xếp hạng cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <returns>200 OK - Trả về bảng xếp hạng</returns>
        [HttpGet("{competitionId}/leaderboard")]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<LeaderboardEntryDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetCompetitionLeaderboard([FromQuery] CompetitionLeaderboardSearchRequest request, Guid competitionId)
        {
            var leaderboard = await _competitionService.GetCompetitionLeaderboard(request, competitionId);
            return SuccessResponse<PaginationResult<IEnumerable<LeaderboardEntryDto>>>.Create(
                leaderboard,
                "Lấy bảng xếp hạng cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Cập nhật trạng thái cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <param name="request">Trạng thái mục tiêu cần cập nhật</param>
        /// <remarks>
        /// API dùng để cập nhật `CompetitionStatus` theo rule domain.
        /// Ví dụ: `REGISTRATION_OPEN`, `REGISTRATION_CLOSED`, `ONGOING`, `FINISHED`, `RESULT_PUBLISHED`, `CANCELLED`, `INVALID`.
        ///
        /// Khi trạng thái được cập nhật sang `INVALID`, hệ thống yêu cầu cung cấp thêm `InvalidReason`
        /// để xác định nguyên nhân cuộc thi không hợp lệ.
        ///
        /// Các giá trị `InvalidReason` bao gồm:
        /// - `NoRounds`: Không có round hợp lệ để tổ chức thi đấu.
        /// - `NoParticipants`: Không có người tham gia cuộc thi.
        /// - `ScheduleInvalid`: Lịch thi đấu không hợp lệ (ngoài khoảng thời gian competition).
        /// - `RegistrationTimeInvalid`: Thời gian đăng ký không hợp lệ.
        /// - `CompetitionTimeInvalid`: Thời gian diễn ra cuộc thi không hợp lệ.
        /// - `StartFailed`: Thất bại khi chuyển trạng thái sang ONGOING.
        /// - `FinishFailed`: Thất bại khi kết thúc cuộc thi.
        /// - `SystemError`: Lỗi hệ thống không xác định.
        /// - `DependencyFailed`: Lỗi từ service bên ngoài (API khác, email, payment,...).
        /// - `Unknown`: Không xác định được nguyên nhân cụ thể.
        ///
        /// Lưu ý:
        /// - `InvalidReason` chỉ áp dụng khi `CompetitionStatus = INVALID`.
        /// - Với các trạng thái khác, trường này có thể được bỏ qua.
        /// </remarks>
        /// <returns>200 OK - Cập nhật trạng thái cuộc thi thành công</returns>
        [HttpPatch("{competitionId}/status")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(CompetitionUpdateStatusDto), typeof(CompetitionUpdateStatusExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> UpdateCompetitionStatus(Guid competitionId, [FromBody] CompetitionUpdateStatusDto request)
        {
            var competition = await _competitionService.UpdateCompetitionStatus(competitionId, request);
            return SuccessResponse<CompetitionResponse>.Create(
                competition,
                "Cập nhật trạng thái cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Thêm certificate vào cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <param name="request">Danh sách certificate IDs cần thêm (chỉ nhận đúng 1 ID)</param>
        /// <remarks>
        /// **Quy tắc:**
        ///
        /// 1. Chỉ cho phép thêm khi Competition ở trạng thái `DRAFT`
        /// 2. Certificate không được trùng trong cùng một competition (trùng sẽ không được chấp nhận)
        /// 3. Certificate phải hợp lệ/tồn tại theo dữ liệu tích hợp từ Academy system
        /// 4. Có thể thêm nhiều certificate trong một request
        /// </remarks>
        /// <returns>200 OK - Thêm certificates thành công</returns>
        [HttpPost("{competitionId}/certificates")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionCertificateAdditionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(CompetitionCertificateAddDto), typeof(CompetitionCertificateAddExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> AddCertificatesToCompetition(
            Guid competitionId,
            [FromBody] CompetitionCertificateAddDto request)
        {
            var result = await _competitionCertificateService.AddCertificateToCompetition(competitionId, request);
            return SuccessResponse<CompetitionCertificateAdditionResponse>.Create(
                result,
                $"Thêm {result.AddedTotal} chứng chỉ vào cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Thêm certificate vào cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <param name="request">Danh sách certificate IDs cần thêm</param>
        /// <remarks>
        /// **Quy tắc:**
        ///
        /// 1. Chỉ cho phép thêm khi Competition ở trạng thái `DRAFT`
        /// 2. Certificate không được trùng trong cùng một competition (trùng sẽ không được chấp nhận)
        /// 3. Certificate phải hợp lệ/tồn tại theo dữ liệu tích hợp từ Academy system
        /// 4. API này chỉ hỗ trợ thêm đúng 1 certificate trong mỗi request
        /// </remarks>
        /// <returns>200 OK - Thêm certificate thành công</returns>
        [HttpPost("{competitionId}/certificates/single")]
        [ProducesResponseType(typeof(SuccessResponse<SimpleCertificateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(CompetitionCertificateAddDto), typeof(CompetitionCertificateAddExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> AddSingleCertificateToCompetition(
            Guid competitionId,
            [FromBody] CompetitionCertificateAddDto request)
        {
            var result = await _competitionCertificateService.AddSingleCertificateToCompetition(competitionId, request);
            return SuccessResponse<SimpleCertificateResponse>.Create(
                result,
                "Thêm chứng chỉ vào cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Lấy danh sách certificates của cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <returns>200 OK - Trả về danh sách certificates</returns>
        [HttpGet("{competitionId}/certificates")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<SimpleCertificateResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetCertificatesByCompetition(Guid competitionId)
        {
            var certificates = await _competitionCertificateService.GetCertificatesByCompetition(competitionId);
            return SuccessResponse<IEnumerable<SimpleCertificateResponse>>.Create(
                certificates,
                "Lấy danh sách certificates thành công!"
            );
        }

        /// <summary>
        /// Xóa nhiều certificate khỏi cuộc thi.
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <param name="request">Danh sách certificate IDs cần xóa</param>
        /// <remarks>
        /// **Quy tắc:**
        ///
        /// 1. Chỉ cho phép xóa certificate khi Competition ở trạng thái `DRAFT`
        /// 2. Chỉ xóa các certificate đang tồn tại trong competition
        /// </remarks>
        /// <returns>200 OK - Xóa certificates thành công</returns>
        [HttpDelete("{competitionId}/certificates")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionCertificateDeletionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> RemoveCertificatesFromCompetition(
            Guid competitionId,
            [FromBody] CompetitionCertificateRemoveDto request)
        {
            var result = await _competitionCertificateService.RemoveCertificatesFromCompetition(competitionId, request);

            return SuccessResponse<CompetitionCertificateDeletionResponse>.Create(
                result,
                $"Xóa {result.DeletedTotal} chứng chỉ khỏi cuộc thi thành công!"
            );
        }

        [HttpGet("{competitionID}/current-round")]
        public async Task<ApiResponse> GetCurrentRoundByCompetitionID(Guid competitionID)
        {
            var result = await _competitionService.GetCurrentRoundByCompetitionID(competitionID);

            throw new NotImplementedException();
        } 
    }
}
