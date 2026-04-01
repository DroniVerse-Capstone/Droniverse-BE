using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface ILessonDuplicator
{
    Task DuplicateAsync(CourseVersionDuplicationContext context);
}
