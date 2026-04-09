using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.QueryModels;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Domain.IRepository;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<Dictionary<Guid, int>> GetActiveOrCompletedParticipantCountsByCourseVersionIdsAsync(
        IEnumerable<Guid> courseVersionIds,
        CancellationToken cancellationToken = default);

    Task<PaginationResult<IEnumerable<CourseEnrollmentQueryModel>>> GetCoursesOfUserAsync(
        Guid userId,
        Guid clubId,
        int pageIndex,
        int pageSize,
        CourseLevel? level = null,
        string? courseSearchName = null,
        EnrollStatus? enrollmentStatus = null,
        CancellationToken cancellationToken = default);
}

