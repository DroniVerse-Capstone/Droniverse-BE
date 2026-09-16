using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.IService.Duplication;

public interface ICourseVersionDuplicator
{
    Task<CourseVersionDuplicationResult> DuplicateAsync(
        Course course,
        CourseVersion sourceVersion,
        int nextVersion,
        Guid currentUserId,
        DateTime now);
}
