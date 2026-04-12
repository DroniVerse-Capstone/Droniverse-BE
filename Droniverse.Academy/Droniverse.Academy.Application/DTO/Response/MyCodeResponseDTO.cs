using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class MyCodeResponseDTO
{
    public string CodeId { get; set; } = string.Empty;
    public Guid ClubId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime ExpireDate { get; set; }
    public CodeStatus Status { get; set; }
    public DateTime? UsedDate { get; set; }
}
