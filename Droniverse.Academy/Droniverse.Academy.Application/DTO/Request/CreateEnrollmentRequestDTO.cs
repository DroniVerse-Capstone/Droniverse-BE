namespace Droniverse.Academy.Application.DTO.Request;

public class CreateEnrollmentRequestDTO
{
    public Guid CourseVersionID { get; set; }
    public Guid? ClubID { get; set; }
}
