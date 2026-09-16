using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserAssignmentService
{
    Task<UserAssignmentSubmitResponseDTO> SubmitAssignmentAsync(Guid enrollmentId, Guid assignmentId, SubmitUserAssignmentRequestDTO request);
    Task<UserAssignmentReviewResponseDTO> ReviewAssignmentAsync(Guid userAssignmentId, ReviewUserAssignmentRequestDTO request);
    Task<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>> GetSubmissionsForReviewAsync(
        Guid? assignmentId,
        Guid? enrollmentId,
        UserAssignmentStatus? status,
        int pageIndex = 1,
        int pageSize = 10);
    Task<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>> GetAssignmentAttemptsByCourseAndClubAsync(
        Guid? courseId,
        Guid? clubId,
        UserAssignmentStatus? status,
        int pageIndex = 1,
        int pageSize = 10);
    Task<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>> GetMyAssignmentAttemptsAsync(
        Guid enrollmentId,
        Guid assignmentId,
        int pageIndex = 1,
        int pageSize = 10);
    Task<AssignmentOverview> GetAssignmentOverView(
    Guid enrollmentId,
    Guid assignmentId);
}
