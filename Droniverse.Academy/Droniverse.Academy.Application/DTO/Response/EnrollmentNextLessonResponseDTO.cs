using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class EnrollmentNextLessonResponseDTO
{
    public Guid EnrollmentID { get; set; }
    public Guid CourseID { get; set; }
    public Guid CourseVersionID { get; set; }
    public Guid ModuleID { get; set; }
    public Guid LessonID { get; set; }
    public int ModuleNumber { get; set; }
    public int OrderIndex { get; set; }
    public LessonType Type { get; set; }
    public Guid ReferenceID { get; set; }
    public UserLessonStatus? Status { get; set; }
    public float? Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }
}
