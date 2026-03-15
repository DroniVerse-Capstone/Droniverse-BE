using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICourseService
{


    /// <summary>
    /// Get course detail by id (Active version only)
    /// </summary>
    Task<CourseResponseDTO> GetCourseByIdActiveAsync(Guid courseId);

    /// <summary>
    /// Get course detail by id (All versions)
    /// </summary>
    Task<CourseDetailResponseDTO> GetCourseByIdAllAsync(Guid courseId);


    /// <summary>
    /// Get paginated course list (Active version only)
    /// </summary>
    Task<PaginationResult<IEnumerable<CourseResponseDTO>>>
        GetAllCoursesActiveAsync(
            int pageIndex,
            int pageSize,
            string? search = null);

    /// <summary>
    /// Get paginated course list (All versions)
    /// </summary>
    Task<PaginationResult<IEnumerable<CourseDetailResponseDTO>>> 
        GetAllCoursesAllAsync(
            int pageIndex,
            int pageSize,
            string? search = null);


    /// <summary>
    /// Create new course (Draft)
    /// </summary>
    Task<CourseDetailResponseDTO> CreateCourseAsync();


    /// <summary>
    /// Publish course (set one version to Active)
    /// </summary>
    Task PublishCourseAsync(Guid courseId);


    /// <summary>
    /// Unpublish course (remove Active version)
    /// </summary>
    Task UnpublishCourseAsync(Guid courseId);


    /// <summary>
    /// Soft delete course
    /// </summary>
    Task DeleteCourseAsync(Guid courseId);
}

