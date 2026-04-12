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
    [HttpGet("{clubId:guid}/codes")]
    [Authorize(Roles = Roles.SystemRoles)]
    public async Task<IActionResult> GetCodesByClub(Guid clubId, [FromQuery] GetAllCodesByClubSearchRequest request)
    {
        var result = await _codeService.GetCodesByClub(clubId, request);
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
    /// Club member nhập mã code để kích hoạt quyền truy cập khóa học.
    /// </summary>
    [HttpPost("enter-code")]
    [Authorize(Roles = Roles.ClubMember)]
    public async Task<IActionResult> EnterCodes([FromQuery] string codeId)
    {
        CodeUsageResponseDTO result = await _codeService.EnterCodeAsync(codeId);
        return Ok(SuccessResponse<CodeUsageResponseDTO>.Create(result, "Truy cập khóa học thành công"));
    }

    /// <summary>
    /// Lấy danh sách code của người dùng hiện tại, có phân trang.
    /// </summary>
    [HttpGet("users/me/codes")]
    [Authorize(Roles = Roles.AllRoles)]
    public async Task<IActionResult> GetCodesByUser([FromQuery] GetCodesByUserSearchRequest request)
    {
        var result = await _codeService.GetCodesByUserAsync(_currentUserService.UserId, request);
        return Ok(SuccessResponse<PaginationResult<IEnumerable<MyCodeResponseDTO>>>.Create(result, "Lấy danh sách code của người dùng thành công."));
    }

    /// <summary>
    /// Gán một mã code cho một người dùng.
    /// </summary>
    [HttpPost("{codeId}/assign")]
    [Authorize(Roles = Roles.SystemRoles)]
    public async Task<IActionResult> AssignCode(string codeId, [FromBody] AssignCodeRequest request)
    {
        var result = await _codeService.AssignCodeAsync(codeId, request);
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

}
