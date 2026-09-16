namespace Droniverse.Academy.Application.DTO.Request;

public class CreateUserLabRequestDTO
{
    public Guid LabID { get; set; }
    public string Solution { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public float Time { get; set; }
    public int NumberOfStep { get; set; }
    public float Length { get; set; }
    public string FeedbackVN { get; set; } = string.Empty;
    public string FeedbackEN { get; set; } = string.Empty;
    public decimal Point { get; set; }
}
