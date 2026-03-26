using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class CourseVersionByDroneClientViewDTO
{
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public string? DescriptionVN { get; set; }
    public string? DescriptionEN { get; set; }
    public CourseVersionStatus Status { get; set; }
    public int Version { get; set; }
    public string? ImageUrl { get; set; }
    public CourseLevel Level { get; set; }
    public int? EstimatedDuration { get; set; }
    public string? ChangeLog { get; set; }
}
