using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public record MyCodeResponseDTO
{
    public string CodeId { get; set; } = string.Empty;
    public required SimpleClubResponse ClubInfo { get; set; }
    public required SimpleCourseResponse CourseId { get; set; }
    public DateTime ExpireDate { get; set; }
    public CodeStatus Status { get; set; }
    public DateTime? UsedDate { get; set; }
}
