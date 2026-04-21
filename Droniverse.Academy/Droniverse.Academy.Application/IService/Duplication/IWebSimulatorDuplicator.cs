using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface IWebSimulatorDuplicator
{
    Task<Guid> DuplicateAsync(Guid sourceWebSimulatorId, CourseVersionDuplicationContext context);
}
