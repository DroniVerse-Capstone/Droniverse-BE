using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class LessonClientViewDTO
{
    public Guid LessonID { get; set; }
    public Guid ModuleID { get; set; }
    public int OrderIndex { get; set; }
    public LessonType Type { get; set; }
    public Guid ReferenceID { get; set; }
    public string? TitleVN { get; set; }
    public string? TitleEN { get; set; }
    public int? EstimatedTime { get; set; }
}
