namespace Droniverse.Academy.Application.DTO.Response;

public class CompleteLessonResultDTO
{
    public Guid EnrollmentID { get; set; }
    public Guid ModuleID { get; set; }
    public Guid LessonID { get; set; }
    public bool IsAlreadyCompleted { get; set; }
    public float ModuleProgress { get; set; }
    public bool IsModuleCompleted { get; set; }
    public float EnrollmentProgress { get; set; }
    public bool IsEnrollmentCompleted { get; set; }
    public bool IsCertificateIssued { get; set; }
}
