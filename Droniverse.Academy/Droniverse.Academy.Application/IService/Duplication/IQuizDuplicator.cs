using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface IQuizDuplicator
{
    Task<Guid> DuplicateAsync(Guid sourceQuizId, CourseVersionDuplicationContext context);
}
