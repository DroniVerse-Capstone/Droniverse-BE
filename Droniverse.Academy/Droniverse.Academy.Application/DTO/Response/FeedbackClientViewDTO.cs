using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public class FeedbackClientViewDTO
{
    public Guid FeedbackID { get; set; }
    public SimpleUserReponse? User { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreateAt { get; set; }
}
