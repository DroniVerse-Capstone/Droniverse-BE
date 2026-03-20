using Droniverse.Academy.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Academy.Application.HttpClients;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/categories")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ILogger<CategoryController> _logger;
    private readonly ICourseVersionCategoryService _service;
    

    public CategoryController(ILogger<CategoryController> logger, ICourseVersionCategoryService service)
    {
        _logger = logger;
        _service = service;
    }

    // GET /academy/categories/{categoryId}/course-versions
    [HttpGet("{categoryId:guid}/course-versions")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetCourseVersionsByCategory(Guid categoryId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50, [FromQuery] bool activeOnly = true)
    {
        try
        {
            var result = await _service.GetCourseVersionsByCategoryAsync(categoryId, pageIndex, pageSize, activeOnly);
            return Ok(SuccessResponse<object>.Create(result, "L?y danh sách course version theo category thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCourseVersionsByCategory failed for {CategoryId}", categoryId);
            throw;
        }
    }

}
