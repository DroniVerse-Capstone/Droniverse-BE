namespace Droniverse.Academy.Application.DTO.Response;

public class ModuleResponseDTO
{
    public Guid ModuleID { get; set; }
    public Guid CourseVersionID { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int ModuleNumber { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
}
