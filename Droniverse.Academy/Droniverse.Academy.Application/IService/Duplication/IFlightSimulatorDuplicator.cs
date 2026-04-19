using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface IFlightSimulatorDuplicator
{
    Task<Guid> DuplicateAsync(Guid sourceFlightId, CourseVersionDuplicationContext context);
}
