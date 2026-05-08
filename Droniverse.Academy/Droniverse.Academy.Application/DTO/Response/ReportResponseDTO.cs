using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.DTO.Response;

public class ReportResponseDTO
{
    public Guid ReportID { get; set; }
    public ReportType ReportType { get; set; }
    public Guid ReferenceID { get; set; }
    public SimpleUserReponse? ReportedUser { get; set; }
    public CourseVersionMiniResponseDTO? ReportedCourseVersion { get; set; }
    public ClubMiniResponseDto? ReportedClub { get; set; }
    public Guid UserID { get; set; }
    public SimpleUserReponse? User { get; set; }
    public string? ContentVN { get; set; }
    public string? ContentEN { get; set; }
    public string? ResponseVN { get; set; }
    public string? ResponseEN { get; set; }
    public Guid? Responser { get; set; }
    public SimpleUserReponse? ResponserUser { get; set; }
}
