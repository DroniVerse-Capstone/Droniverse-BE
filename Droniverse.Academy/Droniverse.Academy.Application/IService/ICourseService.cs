using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICourseService
{
    Task<CourseResponseDTO> GetCourseByIdAsync(Guid courseId);

    Task<PaginationResult<IEnumerable<CourseResponseDTO>>>
        GetAllCoursesAsync(
            int pageIndex,
            int pageSize,
            string? search = null,
            CourseStatus? status = null);

    Task<CourseDetailResponseDTO> CreateCourseAsync();

    Task PublishCourseAsync(Guid courseId);

    Task UnpublishCourseAsync(Guid courseId);

    Task DeleteCourseAsync(Guid courseId);

    Task<IEnumerable<CourseBulkResponseDTO>> GetCoursesByIdsAsync(
        CourseBulkSearchRequest searchRequest,
        IEnumerable<Guid> courseIds);
}

