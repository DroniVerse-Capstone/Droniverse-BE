using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class CourseResponseDTO
{
    public Guid CourseID { get; set; }
    public Guid CreateBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; }
    public CourseStatus Status { get; set; }
    public CourseVersionResponseDTO? CurrentVersion { get; set; }
}

