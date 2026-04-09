using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class LearningPathDTO
{
    public Guid EnrollmentID { get; set; }
    public Guid CourseID { get; set; }
    public Guid CourseVersionID { get; set; }
    public string TitleVN { get; set; } = string.Empty;
    public string TitleEN { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int? Duration { get; set; }
    public float Progress { get; set; }
    public IReadOnlyCollection<LearningPathModuleDTO> Modules { get; set; } = [];
}

public class LearningPathModuleDTO
{
    public Guid ModuleID { get; set; }
    public string TitleVN { get; set; } = string.Empty;
    public string TitleEN { get; set; } = string.Empty;
    public int ModuleNumber { get; set; }
    public int TotalLessons { get; set; }
    public int? Duration { get; set; }
    public float Progress { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; }
    public IReadOnlyCollection<LearningPathLessonDTO> Lessons { get; set; } = [];
}

public class LearningPathLessonDTO
{
    public Guid LessonID { get; set; }
    public int OrderIndex { get; set; }
    public LessonType Type { get; set; }
    public Guid ReferenceID { get; set; }
    public string? TitleVN { get; set; }
    public string? TitleEN { get; set; }
    public int? Duration { get; set; }
    public float Progress { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastAccessDate { get; set; }
}
