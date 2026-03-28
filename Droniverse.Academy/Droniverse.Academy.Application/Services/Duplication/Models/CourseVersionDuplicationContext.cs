using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Services.Duplication.Models;

public class CourseVersionDuplicationContext
{
    public required CourseVersion SourceVersion { get; init; }
    public required CourseVersion DuplicatedVersion { get; init; }
    public required Guid CurrentUserId { get; init; }
    public required DateTime Now { get; init; }

    public Dictionary<Guid, Guid> ModuleIdMap { get; } = new();
    public Dictionary<Guid, Guid> TheoryIdMap { get; } = new();
    public Dictionary<Guid, Guid> QuizIdMap { get; } = new();
    public Dictionary<Guid, Guid> LabIdMap { get; } = new();
    public List<(Guid SourceLabId, Guid NewLabId)> LabContentSyncQueue { get; } = new();
}
