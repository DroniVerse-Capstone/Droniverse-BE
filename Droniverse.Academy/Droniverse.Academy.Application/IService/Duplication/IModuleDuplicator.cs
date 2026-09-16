using Droniverse.Academy.Application.Services.Duplication.Models;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface IModuleDuplicator
{
    Task DuplicateAsync(CourseVersionDuplicationContext context);
}
