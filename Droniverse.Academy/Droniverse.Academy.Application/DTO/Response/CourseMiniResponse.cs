using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public class CourseMiniResponse
{
    public Guid CourseID { get; set; }
    public LevelMiniResponse? Level { get; set; }
    public DroneMiniResponse? Drone { get; set; }
    public ICollection<CourseVersionMiniResponseDTO> CourseVersions { get; set; } = [];
}
