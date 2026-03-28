using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public class QuizClientViewDTO
{
    public Guid QuizID { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public string DescriptionVN { get; set; } = null!;
    public string DescriptionEN { get; set; } = null!;
    public int TimeLimit { get; set; }
    public float TotalScore { get; set; }
    public float PassScore { get; set; }
    public DateTime CreateAt { get; set; }
    public SimpleUserReponse? Creator { get; set; }
    public DateTime UpdateAt { get; set; }
    public SimpleUserReponse? Updater { get; set; }
}
