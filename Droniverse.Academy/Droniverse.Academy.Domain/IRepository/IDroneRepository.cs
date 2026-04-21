using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Domain.IRepository;
public interface IDroneRepository : IRepository<Drone>
{
    Task<IEnumerable<DroneResponseDto>> GetDronesByIdsAsync(IEnumerable<Guid> droneIds);
}

