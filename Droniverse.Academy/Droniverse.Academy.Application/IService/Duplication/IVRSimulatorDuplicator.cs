using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface IVRSimulatorDuplicator
{
    Task<Guid> DuplicateAsync(Guid sourceVRSimulatorId, CourseVersionDuplicationContext context);
}
