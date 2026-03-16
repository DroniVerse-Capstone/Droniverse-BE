namespace Droniverse.Academy.Application.DTO.Request;

public class CreateQuizRequestDTO
{
    public Guid LessonID { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public string DescriptionVN { get; set; } = null!;
    public string DescriptionEN { get; set; } = null!;
    public int TimeLimit { get; set; }
    public float TotalScore { get; set; }
    public float PassScore { get; set; }
}
