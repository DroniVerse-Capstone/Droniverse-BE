using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IWebSimulatorService
{
    Task<WebSimulatorClientViewDTO> CreateWebSimulatorAsync(CreateWebSimulatorRequestDTO request);
    Task<IEnumerable<WebSimulatorClientViewDTO>> GetWebSimulatorsAsync();
    Task<WebSimulatorClientViewDTO> GetWebSimulatorByIdAsync(Guid webSimulatorId);
    Task<WebSimulatorClientViewDTO> UpdateWebSimulatorAsync(Guid webSimulatorId, UpdateWebSimulatorRequestDTO request);
    Task DeleteWebSimulatorAsync(Guid webSimulatorId);
}
