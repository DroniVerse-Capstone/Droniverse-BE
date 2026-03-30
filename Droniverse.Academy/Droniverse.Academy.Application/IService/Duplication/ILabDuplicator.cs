using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface ILabDuplicator
{
    Task<Guid> DuplicateAsync(Guid sourceLabId, CourseVersionDuplicationContext context);
}
