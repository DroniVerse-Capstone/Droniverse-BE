using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IRequiredDroneService
{
    Task<IEnumerable<DroneClientViewDTO>> AddRequiredDronesAsync(Guid courseId, Guid versionId, AddRequiredDronesRequestDTO request);
    Task RemoveRequiredDroneAsync(Guid courseId, Guid versionId, Guid droneId);
    Task<IEnumerable<DroneClientViewDTO>> GetRequiredDronesAsync(Guid courseId, Guid versionId);
    Task<IEnumerable<CourseVersionByDroneClientViewDTO>> GetCourseVersionsByDroneAsync(Guid droneId);
}
