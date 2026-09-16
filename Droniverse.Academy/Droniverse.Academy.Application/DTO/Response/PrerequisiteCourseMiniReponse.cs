namespace Droniverse.Academy.Application.DTO.Response;

public class PrerequisiteCourseMiniReponse
{
    public Guid CourseID { get; set; }
    public LevelMiniResponse? Level { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string TitleVN { get; set; } = string.Empty;
    public string TitleEN { get; set; } = string.Empty;
}
