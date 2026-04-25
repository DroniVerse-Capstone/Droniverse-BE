namespace Droniverse.Academy.Application.DTO.Request;

public class CreateAssignmentRequestDTO
{
    public string TitleEN { get; set; } = string.Empty;
    public string TitleVN { get; set; } = string.Empty;
    public string DescriptionEN { get; set; } = string.Empty;
    public string DescriptionVN { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public int EstimatedTime { get; set; }
}
