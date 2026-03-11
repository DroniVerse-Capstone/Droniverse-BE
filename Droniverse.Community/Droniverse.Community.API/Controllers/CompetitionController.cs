using CloudinaryDotNet.Actions;
using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    /// <summary>
    /// API quản lý cuộc thi (Competition)
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
        /// Tạo cuộc thi mới
        /// </summary>
        /// <param name="request">Thông tin cuộc thi</param>
        /// <returns>200 OK - Tạo cuộc thi thành công</returns>
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
        /// Cập nhật thông tin cuộc thi
        /// </summary>
        /// <param name="id">ID của cuộc thi</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <remarks>
        /// **Quy tắc cập nhật:**
        /// 
        /// 1. **Chỉ cho phép cập nhật khi Status = DRAFT hoặc OPEN**
        ///    - Không thể cập nhật khi cuộc thi đang ở trạng thái CLOSED, ONGOING, FINISHED, hoặc CANCELLED
        /// 
        /// 2. **Khi thay đổi StartDate hoặc EndDate:**
        ///    - Hệ thống sẽ tự động validate lại tất cả các Round có status = PENDING
        ///    - Nếu Round có thời gian nằm ngoài khoảng [Competition.StartDate, Competition.EndDate]:
        ///      → Round sẽ bị đánh dấu status = SCHEDULE_INVALID
        ///    - Các Round bị SCHEDULE_INVALID phải được chỉnh sửa trước khi competition có thể bắt đầu
        /// 
        /// 3. **Validation thời gian:**
        ///    - RegistrationStartDate &lt; RegistrationEndDate
        ///    - StartDate &lt; EndDate
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
        /// Xóa cuộc thi
        /// </summary>
        /// <param name="id">ID của cuộc thi</param>
        /// <remarks>
        /// **Quy tắc xóa:**
        /// 
        /// - Chỉ cho phép xóa cuộc thi ở trạng thái **DRAFT**
        /// - Không thể xóa cuộc thi đã có người đăng ký hoặc đã bắt đầu
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
        /// Lấy thông tin chi tiết cuộc thi theo ID
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
        /// Lấy danh sách cuộc thi với điều kiện lọc
        /// </summary>
        /// <param name="searchRequest">Điều kiện tìm kiếm (status, clubId, searchTerm)</param>
        /// <returns>200 OK - Trả về danh sách cuộc thi</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<CompetitionResponse>>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetAllCompetitionWithCondition([FromQuery] CompetitionSearchRequest searchRequest)
        {
            var competitions = await _competitionService.GetAllCompetitionsWithCondition(searchRequest);
            return SuccessResponse<IEnumerable<CompetitionResponse>>.Create(
                competitions,
                "Lấy danh sách cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Lấy danh sách cuộc thi của một club
        /// </summary>
        /// <param name="clubId">ID của club</param>
        /// <param name="status">Trạng thái cuộc thi (0-DRAFT, 1-OPEN, 2-CLOSED, 3-ONGOING, 4-FINISHED, 5-CANCELLED). Để null để lấy tất cả.</param>
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
        /// Đăng ký tham gia cuộc thi
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
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
        /// Rút khỏi cuộc thi
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
        /// Lấy danh sách thí sinh tham gia cuộc thi
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
        /// Lấy bảng xếp hạng cuộc thi
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <returns>200 OK - Trả về bảng xếp hạng</returns>
        [HttpGet("{competitionId}/leaderboard")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<LeaderboardEntryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetCompetitionLeaderboard(Guid competitionId)
        {
            var leaderboard = await _competitionService.GetCompetitionLeaderboard(competitionId);
            return SuccessResponse<IEnumerable<LeaderboardEntryDto>>.Create(
                leaderboard,
                "Lấy bảng xếp hạng cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Kết thúc cuộc thi
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <returns>200 OK - Kết thúc cuộc thi thành công</returns>
        [HttpPut("{competitionId}/finish")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> FinishCompetition(Guid competitionId)
        {
            var competition = await _competitionService.FinishCompetition(competitionId);
            return SuccessResponse<CompetitionResponse>.Create(
                competition,
                "Kết thúc cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Thêm certificate vào cuộc thi
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <param name="request">Danh sách certificate IDs cần thêm</param>
        /// <remarks>
        /// **Quy tắc:**
        /// 
        /// 1. **Chỉ cho phép thêm certificate khi Competition ở trạng thái DRAFT**
        /// 2. **Certificates không được trùng** - mỗi certificate chỉ được thêm 1 lần vào competition
        /// 3. **Tất cả certificates phải tồn tại trong Academy system** - hệ thống sẽ validate với Academy Microservice
        /// 4. **Có thể thêm nhiều certificates cùng lúc** - gửi danh sách CertificateIDs
        /// 5. **Nếu certificate đã tồn tại sẽ bị skip** - chỉ add những certificate chưa có
        /// 
        /// **Use Case:**
        /// - Admin thiết lập các loại certificate sẽ trao cho thí sinh (Top 1, Top 3, Participation, etc.)
        /// - Certificate phải được thiết lập trước khi mở đăng ký (OPEN)
        /// - Có thể add một lúc nhiều certificates để tiết kiệm thời gian
        /// 
        /// **Performance:**
        /// - Sử dụng Bulk API để validate nhiều certificates cùng lúc
        /// - Parallel validation với Academy service
        /// </remarks>
        /// <returns>200 OK - Thêm certificates thành công</returns>
        [HttpPost("{competitionId}/certificates")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionCertificatesBulkResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(CompetitionCertificateAddDto), typeof(CompetitionCertificateAddExample))]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> AddCertificateToCompetition(
            Guid competitionId,
            [FromBody] CompetitionCertificateAddDto request)
        {
            var result = await _competitionCertificateService.AddCertificateToCompetition(competitionId, request);
            return SuccessResponse<CompetitionCertificatesBulkResponseDto>.Create(
                result,
                $"Thêm {result.TotalAdded} certificate(s) vào cuộc thi thành công!"
            );
        }

        /// <summary>
        /// Lấy danh sách certificates của cuộc thi
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <remarks>
        /// **Trả về:**
        /// - Danh sách tất cả certificates đã được thiết lập cho competition
        /// - Bao gồm thông tin chi tiết từ Academy system (name, description, template URL)
        /// - Sử dụng bulk API để tối ưu performance khi có nhiều certificates
        /// </remarks>
        /// <returns>200 OK - Trả về danh sách certificates</returns>
        [HttpGet("{competitionId}/certificates")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<CompetitionCertificateResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetCertificatesByCompetition(Guid competitionId)
        {
            var certificates = await _competitionCertificateService.GetCertificatesByCompetition(competitionId);
            return SuccessResponse<IEnumerable<CompetitionCertificateResponseDto>>.Create(
                certificates,
                "Lấy danh sách certificates thành công!"
            );
        }

        /// <summary>
        /// Xóa certificate khỏi cuộc thi
        /// </summary>
        /// <param name="competitionId">ID của cuộc thi</param>
        /// <param name="certificateId">ID của certificate cần xóa</param>
        /// <remarks>
        /// **Quy tắc:**
        /// 
        /// 1. **Chỉ cho phép xóa certificate khi Competition ở trạng thái DRAFT**
        /// 2. **Certificate phải tồn tại trong competition** - nếu không sẽ trả về lỗi 404
        /// 
        /// **Use Case:**
        /// - Admin muốn thay đổi cấu hình certificate trước khi mở đăng ký
        /// - Loại bỏ certificate không phù hợp
        /// </remarks>
        /// <returns>200 OK - Xóa certificate thành công</returns>
        [HttpDelete("{competitionId}/certificates/{certificateId}")]
        [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        public async Task<ApiResponse> RemoveCertificateFromCompetition(Guid competitionId, Guid certificateId)
        {
            var result = await _competitionCertificateService.RemoveCertificateFromCompetition(competitionId, certificateId);
            if (!result)
                return ErrorResponse.Create("Xóa certificate thất bại!", "ERR_DELETE_FAILED");

            return SuccessResponse<string>.Create(
                null,
                "Xóa certificate khỏi cuộc thi thành công!"
            );
        }
    }
}
