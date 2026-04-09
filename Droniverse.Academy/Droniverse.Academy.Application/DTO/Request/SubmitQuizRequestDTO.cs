namespace Droniverse.Academy.Application.DTO.Request;

public class SubmitQuizRequestDTO
{
    /// <summary>
    /// Danh sách câu trả lời theo từng câu hỏi.
    /// </summary>
    public IReadOnlyCollection<SubmitQuizAnswerRequestDTO> Answers { get; set; } = [];
}

public class SubmitQuizAnswerRequestDTO
{
    /// <summary>
    /// Mã câu hỏi.
    /// </summary>
    public Guid QuestionID { get; set; }

    /// <summary>
    /// Key đáp án được chọn (A/B/C/D), dùng cho luồng shuffle đáp án.
    /// </summary>
    public string? SelectedOptionKey { get; set; }

    /// <summary>
    /// Trường tương thích cũ. Có thể là A/B/C/D hoặc nội dung đáp án.
    /// </summary>
    public string SelectedAnswer { get; set; } = string.Empty;
}
