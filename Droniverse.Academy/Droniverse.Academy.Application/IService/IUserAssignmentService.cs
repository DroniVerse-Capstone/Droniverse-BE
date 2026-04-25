using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserAssignmentService
{
    Task<UserAssignmentSubmitResponseDTO> SubmitAssignmentAsync(Guid enrollmentId, Guid assignmentId, SubmitUserAssignmentRequestDTO request);
    Task<UserAssignmentReviewResponseDTO> ReviewAssignmentAsync(Guid userAssignmentId, ReviewUserAssignmentRequestDTO request);
}
