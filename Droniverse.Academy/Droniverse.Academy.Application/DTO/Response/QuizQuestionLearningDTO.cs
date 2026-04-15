namespace Droniverse.Academy.Application.DTO.Response;

public class QuizQuestionLearningDTO
{
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int TimeLimit { get; set; }
    public Guid QuestionID { get; set; }
    public string ContentVN { get; set; } = null!;
    public string ContentEN { get; set; } = null!;
    public IReadOnlyList<QuizQuestionOptionLearningDTO> Options { get; set; } = [];
}

public class QuizQuestionOptionLearningDTO
{
    public string OptionKey { get; set; } = null!;
    public string ContentVN { get; set; } = null!;
    public string ContentEN { get; set; } = null!;
}
