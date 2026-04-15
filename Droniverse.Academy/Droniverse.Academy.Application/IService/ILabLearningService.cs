using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ILabLearningService
{
    Task<LabLearningStateDTO> GetLabLearningStateAsync(Guid enrollmentId, Guid labId);
    Task<LabLearningMiniDTO> GetLabLearningMiniAsync(Guid enrollmentId, Guid labId);
    Task<SubmitLabResultDTO> SubmitLabAsync(Guid enrollmentId, Guid labId, SubmitLabRequestDTO request);
}
