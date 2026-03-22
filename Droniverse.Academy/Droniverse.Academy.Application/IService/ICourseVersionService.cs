using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICourseVersionService
{
    Task<CourseVersionResponseDTO> CreateCourseVersionAsync(Guid courseId, CreateCourseVersionRequestDTO request);

    Task<PaginationResult<IEnumerable<CourseVersionResponseDTO>>> GetCourseVersionsAsync(Guid courseId, int pageIndex, int pageSize, CourseVersionStatus? status = null);

    Task<CourseVersionResponseDTO> GetCourseVersionByIdAsync(Guid courseId, Guid versionId);

    Task<CourseVersionResponseDTO> UpdateCourseVersionAsync(Guid courseId, Guid versionId, UpdateCourseVersionRequestDTO request);

    Task DeleteCourseVersionAsync(Guid courseId, Guid versionId);

    Task ActivateCourseVersionAsync(Guid courseId, Guid versionId);

    Task DeactivateCourseVersionAsync(Guid courseId, Guid versionId);
}
