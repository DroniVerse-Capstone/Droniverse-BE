namespace Droniverse.Academy.Application.DTO.Request;

public class ReviewUserAssignmentRequestDTO
{
    public Guid UserId { get; set; }
    public int Score { get; set; }
    public string? ReviewComment { get; set; }
}
