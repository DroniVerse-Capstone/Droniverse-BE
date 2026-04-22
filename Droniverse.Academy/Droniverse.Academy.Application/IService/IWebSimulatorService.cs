using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.IService;

public interface IWebSimulatorService
{
    Task<WebSimulatorClientViewDTO> CreateWebSimulatorAsync(CreateWebSimulatorRequestDTO request);
    Task<LessonClientViewDTO> CreateLessonFromWebSimulatorAsync(Guid webSimulatorId, CreateWebSimulatorLessonRequestDTO request);
    Task<IEnumerable<WebSimulatorClientViewDTO>> GetWebSimulatorsAsync(WebSimulatorType? type = null, Guid? droneId = null);
    Task<WebSimulatorClientViewDTO> GetWebSimulatorByIdAsync(Guid webSimulatorId);
    Task<WebSimulatorClientViewDTO> UpdateWebSimulatorAsync(Guid webSimulatorId, UpdateWebSimulatorRequestDTO request);
    Task DeleteWebSimulatorAsync(Guid webSimulatorId);
}
