using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class CreateCourseVersionRequestDTO
{
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;

    public string? DescriptionVN { get; set; }
    public string? DescriptionEN { get; set; }

    public string? ContextVN { get; set; }
    public string? ContextEN { get; set; }

    public string? ImageUrl { get; set; }


    public int? EstimatedDuration { get; set; }

    public string? ChangeLog { get; set; }
}
