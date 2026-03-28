using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface ILessonReferenceDuplicator
{
    Task<Guid> DuplicateAsync(Lesson sourceLesson, CourseVersionDuplicationContext context);
}
