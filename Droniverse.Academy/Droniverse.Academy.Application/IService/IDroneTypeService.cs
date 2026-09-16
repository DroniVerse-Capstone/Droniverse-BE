using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.IService;

public interface IDroneTypeService
{
    Task<DroneTypeClientViewDTO> CreateDroneTypeAsync(CreateDroneTypeRequestDTO request);
    Task<IEnumerable<DroneTypeClientViewDTO>> GetDroneTypesAsync();
    Task<DroneTypeClientViewDTO> GetDroneTypeByIdAsync(Guid droneTypeId);
    Task<DroneTypeClientViewDTO> UpdateDroneTypeAsync(Guid droneTypeId, UpdateDroneTypeRequestDTO request);
    Task DeleteDroneTypeAsync(Guid droneTypeId);

    Task<DroneClientViewDTO> CreateDroneAsync(Guid droneTypeId, CreateDroneRequestDTO request);
    Task<IEnumerable<DroneClientViewDTO>> GetDronesByTypeAsync(Guid droneTypeId, DroneStatus? status = null);
}
