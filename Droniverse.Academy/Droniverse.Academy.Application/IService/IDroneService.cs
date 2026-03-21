using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.IService;

public interface IDroneService
{
    Task<IEnumerable<DroneClientViewDTO>> GetDronesAsync(DroneStatus? status = null);
    Task<DroneClientViewDTO> GetDroneByIdAsync(Guid droneId);
    Task<DroneClientViewDTO> UpdateDroneAsync(Guid droneId, UpdateDroneRequestDTO request);
    Task DeleteDroneAsync(Guid droneId);
}
