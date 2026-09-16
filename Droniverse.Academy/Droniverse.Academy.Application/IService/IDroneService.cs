using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IDroneService
{
    Task<IEnumerable<DroneClientViewDTO>> GetDronesAsync(Domain.Enums.DroneStatus? status = null);
    Task<DroneClientViewDTO> UpdateDroneAsync(Guid droneId, UpdateDroneRequestDTO request);
    Task DeleteDroneAsync(Guid droneId);
    Task<IEnumerable<DroneResponseDto>> GetDronesByIdsAsync(IEnumerable<Guid> droneIds);
    Task<DroneClientViewDTO> GetDroneByIdAsync(Guid droneId);
}
