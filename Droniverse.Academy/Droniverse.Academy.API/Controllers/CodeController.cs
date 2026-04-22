using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/codes")]
[ApiController]
public class CodeController : ControllerBase
{
    private readonly ILogger<CodeController> _logger;
    private readonly ICodeService _codeService;
    private readonly ICurrentUserService _currentUserService;


    public CodeController(ILogger<CodeController> logger, ICodeService service, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _codeService = service;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Lấy danh sách mã code theo bộ lọc quản trị.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetAllCodes([FromQuery] CodeSearchRequestDTO requestDTO)
    {
        PaginationResult<IEnumerable<CodeResponseDTO>> list = await _codeService.GetAllCodesAsync(requestDTO);
        return Ok(SuccessResponse<PaginationResult<IEnumerable<CodeResponseDTO>>>.Create(list, "Lấy danh sách code thành công."));
    }

    /// <summary>
    /// Lấy chi tiết một mã code.
    /// </summary>
    [HttpGet("{codeId}")]
    [Authorize(Roles = Roles.AllRoles)]
    public async Task<IActionResult> GetCode(string codeId)
    {
        CodeResponseDTO? codeResponseDTO = await _codeService.GetCodeAsync(codeId);
        return Ok(SuccessResponse<CodeResponseDTO>.Create(codeResponseDTO, "Lấy thông tin code thành công."));
    }

    /// <summary>
    /// Lấy danh sách code theo club, có phân trang và lọc trạng thái sử dụng.
    /// </summary>
    [HttpGet("{clubId:guid}/courses/{courseId:guid}/codes")]
    [Authorize(Roles = Roles.SystemRoles)]
    public async Task<IActionResult> GetCodesByClub(Guid clubId, Guid courseId, [FromQuery] GetAllCodesByClubSearchRequest request)
    {
        var result = await _codeService.GetCodesByClub(clubId, courseId, request);
        return Ok(SuccessResponse<ClubCodesResponse>.Create(result, "Lấy danh sách code theo câu lạc bộ thành công."));
    }


    /// <summary>
    /// Tạo mới nhiều mã code cho một khóa học.
    /// </summary>
    [HttpPost("generate")]
    [Authorize(Roles = Roles.SystemRoles)]
    public async Task<IActionResult> CreateCode([FromBody] GenerateCodesRequestDTO request)
    {
        var response = await _codeService.CreateCodeAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Api để call chéo service
    /// </summary>
    [HttpPost("generate-assign")]
    //[Authorize(Roles = Roles.ClubMember)]
    public async Task<IActionResult> GenerateCode([FromBody] Shared.DTOs.Request.GenerateWithAssignCodeRequestDTO request)
    {
        CodeResponseDTO response = await _codeService.CreateWithAssignCodeAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Club member nhập mã code để kích hoạt quyền truy cập khóa học.
    /// </summary>
    [HttpPost("{clubId:guid}/enter-code")]
    [Authorize(Roles = Roles.ClubMember)]
    public async Task<IActionResult> EnterCodes(Guid clubId, [FromBody] EnterCodeRequest request)
    {
        CodeUsageResponseDTO result = await _codeService.EnterCodeAsync(clubId, request.CodeId);
        return Ok(SuccessResponse<CodeUsageResponseDTO>.Create(result, "Truy cập khóa học thành công"));
    }

    /// <summary>
    /// Lấy danh sách code của người dùng hiện tại, có phân trang.
    /// </summary>
    [HttpGet("users/me/codes")]
    [Authorize(Roles = Roles.ClubMember)]
    public async Task<IActionResult> GetCodesByUser([FromQuery] GetCodesByUserSearchRequest request)
    {
        var result = await _codeService.GetCodesByUserAsync(request);
        return Ok(SuccessResponse<PaginationResult<IEnumerable<MyCodeResponseDTO>>>.Create(result, "Lấy danh sách code của người dùng thành công."));
    }

    /// <summary>
    /// Gán một mã code cho một người dùng.
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Roles = Roles.SystemRoles)]
    public async Task<IActionResult> AssignCode([FromBody] AssignCodeRequest request)
    {
        var result = await _codeService.AssignCodeAsync(request);
        return Ok(SuccessResponse<CodeAssignmentResponseDTO>.Create(result, "Gán code thành công."));
    }

    /// <summary>
    /// Gán hàng loạt mã code cho người dùng.
    /// </summary>
    [HttpPost("bulk-assign")]
    [Authorize(Roles = Roles.SystemRoles)]
    public async Task<IActionResult> BulkAssignCodes([FromBody] BulkAssignCodesRequest request)
    {
        var result = await _codeService.BulkAssignCodesAsync(request);
        return Ok(SuccessResponse<BulkCodeAssignmentResponseDTO>.Create(result, "Gán hàng loạt code thành công."));
    }

    /// <summary>
    /// Lấy danh sách thành viên có/không có mã code theo câu lạc bộ và phiên bản khóa học.
    /// </summary>
    [HttpGet("clubs/{clubId:guid}/course/{courseId:guid}/users-codes")]
    [Authorize(Roles = Roles.SystemRoles)]
    public async Task<IActionResult> GetUsersCodes(
        Guid clubId,
        Guid courseId,
        [FromQuery] GetUsersNoCodesSearchRequest request)
    {
        var result = await _codeService.GetUsersCode(clubId, courseId, request);
        return Ok(SuccessResponse<PaginationResult<IEnumerable<SimpleUserReponse>>>.Create(result, "Lấy danh sách người dùng theo trạng thái sở hữu code thành công."));
    }


    /// <summary>
    /// Thành viên nhận mã code miễn phí của khóa học trong câu lạc bộ.
    /// </summary>
    [HttpPost("clubs/{clubId:guid}/courses/{courseId:guid}/get-code")]
    [Authorize(Roles = Roles.ClubMember)]
    public async Task<IActionResult> GetCodeByUsers(Guid clubId, Guid courseId)
    {
        var result = await _codeService.GetCodeByUsers(clubId, courseId);
        return Ok(SuccessResponse<GetCodeByUsersResponseDTO>.Create(result, "Nhận mã code thành công."));
    }
}
