using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICourseVersionCategoryService
{
    Task AddCategoryAsync(Guid courseId, Guid versionId, AssignCategoryRequestDTO request);

    Task RemoveCategoryAsync(Guid courseId, Guid versionId, Guid categoryId);

    Task<PaginationResult<IEnumerable<CategoryResponseDTO>>> GetCategoriesAsync(Guid courseId, Guid versionId, int pageIndex = 1, int pageSize = 50);

    Task<PaginationResult<IEnumerable<CourseVersionResponseDTO>>> GetCourseVersionsByCategoryAsync(Guid categoryId, int pageIndex = 1, int pageSize = 50, bool activeOnly = true);
}
