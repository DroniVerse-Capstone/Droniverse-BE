namespace Droniverse.Academy.Application.DTO.Response;

public class EnrollmentAccessUpdateResponseDTO
{
    public Guid UserId { get; set; }
    public int UpdatedEnrollments { get; set; }
    public int UpdatedUserLessons { get; set; }
    public int SkippedCompletedLessons { get; set; }
    public int SkippedDroppedEnrollments { get; set; }
}
