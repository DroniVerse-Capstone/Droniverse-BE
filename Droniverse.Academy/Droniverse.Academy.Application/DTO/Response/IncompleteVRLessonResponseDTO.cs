using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class IncompleteVRLessonResponseDTO
{
    public Guid UserLessonID { get; set; }
    public Guid LessonID { get; set; }
    public Guid ModuleID { get; set; }
    public Guid EnrollmentID { get; set; }
    public int OrderIndex { get; set; }
    public Guid ReferenceID { get; set; }
    public LessonType Type { get; set; }
    public string? TitleVN { get; set; }
    public string? TitleEN { get; set; }
    public int? EstimatedTime { get; set; }
    public UserLessonStatus Status { get; set; }
    public float Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }
}
