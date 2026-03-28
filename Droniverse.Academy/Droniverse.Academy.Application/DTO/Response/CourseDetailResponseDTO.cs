using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public class CourseDetailResponseDTO
{
    public Guid CourseID { get; set; }
    public SimpleUserReponse? Creator { get; set; }
    public DateTime CreateAt { get; set; }
    public CourseStatus Status { get; set; }
    public CourseVersionResponseDTO? CurrentVersion { get; set; }
    public ICollection<CourseVersionResponseDTO> CourseVersions { get; set; } = [];
}
