using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class AdminUpdateEnrollmentRequestDTO
{
    public float? Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public EnrollStatus? Status { get; set; }
}
