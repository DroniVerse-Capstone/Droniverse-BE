using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/categories")]
    [ApiController]
    public class CategoryController : Controller
    {
        private ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ApiResponse> GetAllCategories()
        {
            try
            {
                return SuccessResponse<IEnumerable<CategoryResponseDto>>
                    .Create(await _categoryService.GetAllCategory(), "Get categories successfully !");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER1001");
            }
        }

        [HttpGet("/{id}")]
        public async Task<ApiResponse> GetCategoryById(Guid id)
        {
            try
            {
                return SuccessResponse<CategoryResponseDto>
                    .Create(await _categoryService.GetCategoryById(id), $"Get category with id [{id}] successfully !");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }

        [HttpDelete("/{id}")]
        public async Task<ApiResponse> DeleteCategory(Guid id)
        {
            try
            {
                var deleted = await _categoryService.Delete(id);

                if (!deleted)
                {
                    return ErrorResponse.Create("Delete fail!", "Err91");
                }

                return SuccessResponse<string>.Create(null, $"Delete category with id [{id}] successfully!");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }

        [HttpPut("/{id}")]
        public async Task<ApiResponse> UpdateCategory(Guid id
            , [FromBody] CategoryRequestDto request)
        {
            try
            {
                return SuccessResponse<CategoryResponseDto>
                    .Create(await _categoryService.UpdateCategory(id, request), $"Update category with id [{id}] successfully !");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }

        [HttpPost]
        public async Task<ApiResponse> CreateCategory([FromBody] CategoryRequestDto request)
        {
            try
            {
                return SuccessResponse<CategoryResponseDto>
                    .Create(await _categoryService.CreateCategory(request), $"Create category successfully !");
            }
            catch (Exception ex)
            {
                return ErrorResponse.Create(ex.Message, "ER102");
            }
        }
    }
}
