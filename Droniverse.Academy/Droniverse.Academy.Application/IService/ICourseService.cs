using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICourseService
{


    /// <summary>
    /// Get course detail by id with current version.
    /// </summary>
    Task<CourseResponseDTO> GetCourseByIdAsync(Guid courseId);


    /// <summary>
    /// Get paginated course list with current version and optional course status filter.
    /// </summary>
    Task<PaginationResult<IEnumerable<CourseResponseDTO>>>
        GetAllCoursesAsync(
            int pageIndex,
            int pageSize,
            string? search = null,
            CourseStatus? status = null);


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

    Task<IEnumerable<CourseResponseDTO>> GetCoursesByIdsAsync(IEnumerable<Guid> courseIds);
}

