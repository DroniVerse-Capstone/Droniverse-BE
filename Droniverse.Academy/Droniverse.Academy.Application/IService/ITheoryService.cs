using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ITheoryService
{
    Task<TheoryClientViewDTO> CreateTheoryAsync(CreateTheoryRequestDTO request);
    Task<IEnumerable<TheoryClientViewDTO>> GetTheoriesAsync();
    Task<TheoryClientViewDTO> GetTheoryByIdAsync(Guid theoryId);
    Task<TheoryClientViewDTO> UpdateTheoryAsync(Guid theoryId, UpdateTheoryRequestDTO request);
    Task DeleteTheoryAsync(Guid theoryId);
}
