using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IDroneService
{
    Task<IEnumerable<DroneClientViewDTO>> GetDronesAsync();
    Task<DroneClientViewDTO> GetDroneByIdAsync(Guid droneId);
    Task<DroneClientViewDTO> UpdateDroneAsync(Guid droneId, UpdateDroneRequestDTO request);
    Task DeleteDroneAsync(Guid droneId);
}
