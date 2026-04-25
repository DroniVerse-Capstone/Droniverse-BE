using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IAssignmentService
{
    Task<AssignmentClientViewDTO> CreateAssignmentAsync(CreateAssignmentRequestDTO request);
}
