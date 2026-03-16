using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/theories")]
[ApiController]
public class TheoryController : ControllerBase
{
    private readonly ILogger<TheoryController> _logger;
    private readonly ITheoryService _theoryService;

    public TheoryController(ILogger<TheoryController> logger, ITheoryService theoryService)
    {
        _logger = logger;
        _theoryService = theoryService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateTheory([FromBody] CreateTheoryRequestDTO request)
    {
        try
        {
            var created = await _theoryService.CreateTheoryAsync(request);
            return StatusCode(201, SuccessResponse<TheoryClientViewDTO>.Create(created, "T?o theory thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateTheory failed");
            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetTheories()
    {
        try
        {
            var theories = await _theoryService.GetTheoriesAsync();
            return Ok(SuccessResponse<IEnumerable<TheoryClientViewDTO>>.Create(theories, "L?y danh sách theory thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTheories failed");
            throw;
        }
    }

    [HttpGet("{theoryId:guid}")]
    [Authorize(Roles = $"{Roles.AdminOrSystemManager},{Roles.ClubMember}")]
    public async Task<IActionResult> GetTheoryById(Guid theoryId)
    {
        try
        {
            var theory = await _theoryService.GetTheoryByIdAsync(theoryId);
            return Ok(SuccessResponse<TheoryClientViewDTO>.Create(theory, "L?y chi ti?t theory thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTheoryById failed for {TheoryId}", theoryId);
            throw;
        }
    }

    [HttpPut("{theoryId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> UpdateTheory(Guid theoryId, [FromBody] UpdateTheoryRequestDTO request)
    {
        try
        {
            var updated = await _theoryService.UpdateTheoryAsync(theoryId, request);
            return Ok(SuccessResponse<TheoryClientViewDTO>.Create(updated, "C?p nh?t theory thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateTheory failed for {TheoryId}", theoryId);
            throw;
        }
    }

    [HttpDelete("{theoryId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> DeleteTheory(Guid theoryId)
    {
        try
        {
            await _theoryService.DeleteTheoryAsync(theoryId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa theory thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteTheory failed for {TheoryId}", theoryId);
            throw;
        }
    }
}
