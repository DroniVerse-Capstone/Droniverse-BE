using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDTO> CreateEnrollmentAsync(CreateEnrollmentRequestDTO request);
    Task<PaginationResult<IEnumerable<EnrollmentResponseDTO>>> GetMyEnrollmentsAsync(int pageIndex = 1, int pageSize = 10, EnrollStatus? status = null);
    Task<EnrollmentResponseDTO> GetMyEnrollmentByIdAsync(Guid enrollmentId);
    Task<EnrollmentResponseDTO> GetMyEnrollmentByClubAndCourseVersionAsync(Guid clubId, Guid courseVersionId);
    Task<EnrollmentResponseDTO> UpdateMyEnrollmentAsync(Guid enrollmentId, UpdateEnrollmentRequestDTO request);
    Task<PaginationResult<IEnumerable<CoursesEnrollmentResponse>>> GetCoursesOfUser(Guid clubId, UserEnrollmentSearchRequest request);
    Task<PaginationResult<IEnumerable<CoursesEnrollmentResponse>>> GetEnrollmentsByClubAsync(Guid clubId, int pageIndex, int pageSize, Guid? courseId = null, Guid? userId = null);
    Task<EnrollmentAccessUpdateResponseDTO> LimitUserAccessAsync(Guid userId);
    Task DeleteMyEnrollmentAsync(Guid enrollmentId);
}
