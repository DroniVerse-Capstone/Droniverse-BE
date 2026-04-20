using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface IStructureSimulatorDuplicator
{
    Task<Guid> DuplicateAsync(Guid sourceStructureId, CourseVersionDuplicationContext context);
}
