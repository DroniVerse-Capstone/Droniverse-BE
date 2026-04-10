using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
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

    Task<CourseOverviewResponseDTO> GetCourseOverviewAsync(
        Guid clubId,
        Guid courseVersionId,
        CancellationToken cancellationToken = default);

    Task<CourseDetailResponseDTO> CreateCourseAsync();

    Task PublishCourseAsync(Guid courseId);

    Task UnpublishCourseAsync(Guid courseId);

    Task DeleteCourseAsync(Guid courseId);

    Task<PagedCourseBulkResponse> GetCoursesByIdsAsync(
        CourseBulkSearchRequest searchRequest,
        IEnumerable<Guid> courseIds);

    Task<PagedCourseBulkResponse> GetHotCoursesByIdsAsync(
        HotCoursesSearchRequest searchRequest,
        IEnumerable<Guid> courseIds);

    Task<PagedManagerCoursesBulkResponse> GetCoursesByIdsManagementAsync(ManagerCourseBulkSearchRequest searchRequest, GetCoursesByIdsRequestDTO courseIds);
}

