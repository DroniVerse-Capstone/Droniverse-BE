using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IAdminEnrollmentService
{
    Task<PaginationResult<IEnumerable<EnrollmentResponseDTO>>> GetEnrollmentsAsync(int pageIndex = 1, int pageSize = 10, Guid? userId = null, Guid? courseVersionId = null, Guid? droneId = null, Guid? levelId = null, Guid? clubId = null, EnrollStatus? status = null);
    Task<EnrollmentResponseDTO> GetEnrollmentByIdAsync(Guid enrollmentId);
    Task<EnrollmentResponseDTO> UpdateEnrollmentAsync(Guid enrollmentId, AdminUpdateEnrollmentRequestDTO request);
    Task DeleteEnrollmentAsync(Guid enrollmentId);
}
