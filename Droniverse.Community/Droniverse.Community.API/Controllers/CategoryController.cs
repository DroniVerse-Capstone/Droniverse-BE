using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/categories")]
    [ApiController]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ICurrentUserService _currentUserService;

        public CategoryController(ICategoryService categoryService, ICurrentUserService currentUserService)
        {
            _categoryService = categoryService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách danh mục trong hệ thống.
        /// </summary>
        /// <returns>
        /// Trả về danh sách các danh mục dưới dạng <see cref="CategoryResponseDto"/>.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(SuccessResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(SuccessResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(SuccessResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ApiResponse> CreateCategory([FromBody] CategoryRequestDto request)
        {
            var category = await _categoryService.CreateCategory(request);

            return SuccessResponse<CategoryResponseDto>
                .Create(category, "Tạo danh mục thành công!");
        }

        //[HttpGet("current-user")]
        //[ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //public ApiResponse GetCurrentUser()
        //{
        //    if (!_currentUserService.IsAuthenticated)
        //    {
        //        return ErrorResponse.Create("Người dùng chưa xác thực!", "Err401");
        //    }

        //    var userInfo = new
        //    {
        //        UserId = _currentUserService.UserID,
        //        UserName = _currentUserService.UserName,
        //        Email = _currentUserService.Email,
        //        Roles = _currentUserService.Roles,
        //        IsAuthenticated = _currentUserService.IsAuthenticated
        //    };

        //    return SuccessResponse<object>
        //        .Create(userInfo, "Lấy thông tin người dùng thành công!");
        //}
    }
}