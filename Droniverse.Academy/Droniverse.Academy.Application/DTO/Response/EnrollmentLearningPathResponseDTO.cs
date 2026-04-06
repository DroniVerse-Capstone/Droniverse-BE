using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class EnrollmentLearningPathResponseDTO
{
    public Guid EnrollmentID { get; set; }
    public Guid CourseID { get; set; }
    public Guid CourseVersionID { get; set; }
    public float Progress { get; set; }
    public IReadOnlyCollection<EnrollmentLearningPathModuleDTO> Modules { get; set; } = [];
}

public class EnrollmentLearningPathModuleDTO
{
    public Guid ModuleID { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int ModuleNumber { get; set; }
    public IReadOnlyCollection<EnrollmentLearningPathLessonDTO> Lessons { get; set; } = [];
}

public class EnrollmentLearningPathLessonDTO
{
    public Guid LessonID { get; set; }
    public int OrderIndex { get; set; }
    public LessonType Type { get; set; }
    public Guid ReferenceID { get; set; }
    public UserLessonStatus? Status { get; set; }
    public float? Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }
}
