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
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách danh mục trong hệ thống.
        /// </summary>
        /// <returns>
        /// Trả về danh sách các danh mục dưới dạng <see cref="CategoryResponseDto"/>.
        /// </returns>
        [HttpGet]
        public async Task<ApiResponse> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategory();

            return SuccessResponse<IEnumerable<CategoryResponseDto>>
                .Create(categories, "Lấy danh sách danh mục thành công!");
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một danh mục theo ID.
        /// </summary>
        /// <param name="id">ID của danh mục cần truy vấn.</param>
        /// <returns>
        /// Trả về thông tin danh mục dưới dạng <see cref="CategoryResponseDto"/>.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetCategoryById(Guid id)
        {
            var category = await _categoryService.GetCategoryById(id);

            return SuccessResponse<CategoryResponseDto>
                .Create(category, $"Lấy danh mục với ID [{id}] thành công!");
        }

        /// <summary>
        /// Xóa một danh mục theo ID.
        /// </summary>
        /// <param name="id">ID của danh mục cần xóa.</param>
        /// <returns>
        /// Trả về kết quả xóa danh mục.
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<ApiResponse> DeleteCategory(Guid id)
        {
            var deleted = await _categoryService.Delete(id);

            if (!deleted)
            {
                return ErrorResponse.Create("Xóa danh mục thất bại!", "Err91");
            }

            return SuccessResponse<string>
                .Create(null, $"Xóa danh mục với ID [{id}] thành công!");
        }

        /// <summary>
        /// Cập nhật thông tin danh mục theo ID.
        /// </summary>
        /// <param name="id">ID của danh mục cần cập nhật.</param>
        /// <param name="request">Thông tin danh mục mới.</param>
        /// <returns>
        /// Trả về thông tin danh mục sau khi được cập nhật.
        /// </returns>
        [HttpPut("{id}")]
        public async Task<ApiResponse> UpdateCategory(Guid id, [FromBody] CategoryRequestDto request)
        {
            var category = await _categoryService.UpdateCategory(id, request);

            return SuccessResponse<CategoryResponseDto>
                .Create(category, $"Cập nhật danh mục với ID [{id}] thành công!");
        }

        /// <summary>
        /// Tạo mới một danh mục.
        /// </summary>
        /// <param name="request">Thông tin danh mục cần tạo.</param>
        /// <returns>
        /// Trả về thông tin danh mục sau khi được tạo.
        /// </returns>
        [HttpPost]
        public async Task<ApiResponse> CreateCategory([FromBody] CategoryRequestDto request)
        {
            var category = await _categoryService.CreateCategory(request);

            return SuccessResponse<CategoryResponseDto>
                .Create(category, "Tạo danh mục thành công!");
        }
    }
}