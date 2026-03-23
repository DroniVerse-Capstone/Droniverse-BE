using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface ILabService
{
    Task<LabDetailResponseDTO> CreateLabAsync(CreateLabRequestDTO request);
    Task<LessonClientViewDTO> CreateLessonFromLabAsync(Guid labId, CreateLabLessonRequestDTO request);
    Task<PaginationResult<IEnumerable<LabClientViewDTO>>> GetLabsAsync(GetLabsQueryDTO query);
    Task<LabDetailResponseDTO> GetLabByIdAsync(Guid labId);
    Task<LabDetailResponseDTO> UpdateLabAsync(Guid labId, UpdateLabRequestDTO request);
    Task<LabContentResponseDTO> UpdateLabContentAsync(Guid labId, UpdateLabContentRequestDTO request);
    Task DeleteLabAsync(Guid labId);
}
