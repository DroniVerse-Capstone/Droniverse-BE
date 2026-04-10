using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ILabLearningService
{
    Task<SubmitLabResultDTO> SubmitLabAsync(Guid enrollmentId, Guid labId, SubmitLabRequestDTO request);
}
