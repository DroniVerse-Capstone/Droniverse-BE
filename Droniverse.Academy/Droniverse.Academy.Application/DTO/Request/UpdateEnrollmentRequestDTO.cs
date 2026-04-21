namespace Droniverse.Academy.Application.DTO.Request;

public class UpdateEnrollmentRequestDTO
{
    public float? Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }
    public DateTime? ExpireDate { get; set; }
}
