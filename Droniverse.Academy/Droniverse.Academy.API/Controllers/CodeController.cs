using Droniverse.Academy.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/codes")]
[ApiController]
public class CodeController : ControllerBase
{
    private readonly ILogger<CodeController> _logger;
    private readonly ICodeService _codeService;


    public CodeController(ILogger<CodeController> logger, ICodeService service)
    {
        _logger = logger;
        _codeService = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCodes()
    {
        PaginationResult<IEnumerable<CodeResponseDTO>> list = await _codeService.GetAllCodesAsync();
        return Ok(SuccessResponse<PaginationResult<IEnumerable<CodeResponseDTO>>>.Create(list, "Lấy danh sách code thành công."));
    }

    [HttpGet("{codeId}")]
    public async Task<IActionResult> GetCode(string codeId)
    {
        CodeResponseDTO? codeResponseDTO = await _codeService.GetCodeAsync(codeId);
        return Ok(SuccessResponse<CodeResponseDTO>.Create(codeResponseDTO, "Lấy thông tin code thành công."));
    }

    [HttpPost("generate-codes")]
    public async Task<IActionResult> CreateCode(Guid courseId, int quantity)
    {
        IEnumerable<string> listIds = await _codeService.CreateCodeAsync(courseId, quantity);
        CodeCreateResponseDTO responseDTO = new CodeCreateResponseDTO
        {
            CourseId = courseId,
            TotalCreated = listIds.Count()
        };
        return Ok(SuccessResponse<CodeCreateResponseDTO>.Create(responseDTO, "Tạo code thành công."));
    }

    [HttpPost("enter-codes")]
    public async Task<IActionResult> EnterCodes(string codeId)
    {
        CodeUsageResponseDTO result = await _codeService.EnterCodeAsync(codeId);
        return Ok(SuccessResponse<CodeUsageResponseDTO>.Create(result, "Truy cập khóa học thành công"));
    }


}
