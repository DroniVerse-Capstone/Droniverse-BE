using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService.Mongo;

public interface ILabContentService
{
    Task<LabContentResponseDTO> CreateEmptyAsync(Guid labId, CancellationToken cancellationToken = default);
    Task<LabContentResponseDTO?> GetByLabIdAsync(Guid labId, CancellationToken cancellationToken = default);
    Task<LabContentResponseDTO> UpdateByLabIdAsync(Guid labId, UpdateLabContentRequestDTO request, CancellationToken cancellationToken = default);
    Task<bool> DeleteByLabIdAsync(Guid labId, CancellationToken cancellationToken = default);
}
