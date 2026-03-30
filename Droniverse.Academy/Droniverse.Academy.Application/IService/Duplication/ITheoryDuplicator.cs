using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface ITheoryDuplicator
{
    Task<Guid> DuplicateAsync(Guid sourceTheoryId, CourseVersionDuplicationContext context);
}
