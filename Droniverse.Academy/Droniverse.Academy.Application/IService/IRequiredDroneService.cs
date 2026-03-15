using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IRequiredDroneService
{
    Task<DroneClientViewDTO> AddRequiredDroneAsync(Guid courseId, Guid versionId, AddRequiredDroneRequestDTO request);
    Task RemoveRequiredDroneAsync(Guid courseId, Guid versionId, Guid droneId);
    Task<IEnumerable<DroneClientViewDTO>> GetRequiredDronesAsync(Guid courseId, Guid versionId);
    Task<IEnumerable<CourseVersionByDroneClientViewDTO>> GetCourseVersionsByDroneAsync(Guid droneId);
}
