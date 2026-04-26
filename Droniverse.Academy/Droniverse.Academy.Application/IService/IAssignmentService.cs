using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IAssignmentService
{
    Task<AssignmentClientViewDTO> CreateAssignmentAsync(CreateAssignmentRequestDTO request);
    Task<PaginationResult<IEnumerable<AssignmentClientViewDTO>>> GetAssignmentsAsync(int pageIndex = 1, int pageSize = 10);
    Task<AssignmentClientViewDTO> GetAssignmentByIdAsync(Guid assignmentId);
    Task<AssignmentClientViewDTO> UpdateAssignmentAsync(Guid assignmentId, UpdateAssignmentRequestDTO request);
    Task DeleteAssignmentAsync(Guid assignmentId);
}
