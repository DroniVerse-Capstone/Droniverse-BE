using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class CourseVersionResponseDTO
{
    public Guid CourseVersionID { get; set; }
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
    public Guid? UpdateBy { get; set; }
    public string? Updater { get; set; }
    public DateTime? UpdateAt { get; set; }
    public string? ContextVN { get; set; }
    public string? ContextEN { get; set; }
    public IEnumerable<CategoryResponseDTO> Categories { get; set; } = [];
    public IEnumerable<RequiredDroneResponseDTO> RequiredDrones { get; set; } = [];
}

