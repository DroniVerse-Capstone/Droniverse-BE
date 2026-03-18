using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ILabService
{
    Task<LabClientViewDTO> CreateLabAsync(CreateLabRequestDTO request);
    Task<IEnumerable<LabClientViewDTO>> GetLabsAsync();
    Task<LabClientViewDTO> GetLabByIdAsync(Guid labId);
    Task<LabClientViewDTO> UpdateLabAsync(Guid labId, UpdateLabRequestDTO request);
    Task DeleteLabAsync(Guid labId);
}
