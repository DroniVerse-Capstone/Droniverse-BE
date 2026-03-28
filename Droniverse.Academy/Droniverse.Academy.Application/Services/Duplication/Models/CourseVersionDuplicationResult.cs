using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Services.Duplication.Models;

public class CourseVersionDuplicationResult
{
    public required CourseVersion DuplicatedVersion { get; init; }
    public required IReadOnlyCollection<(Guid SourceLabId, Guid NewLabId)> LabContentSyncQueue { get; init; }
}
