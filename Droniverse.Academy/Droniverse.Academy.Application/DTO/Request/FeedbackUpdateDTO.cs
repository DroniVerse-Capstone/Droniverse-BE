namespace Droniverse.Academy.Application.DTO.Request;

public class FeedbackUpdateDTO
{
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
}
