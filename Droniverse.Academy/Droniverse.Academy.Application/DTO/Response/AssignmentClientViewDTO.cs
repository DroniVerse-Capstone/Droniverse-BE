namespace Droniverse.Academy.Application.DTO.Response;

public class AssignmentClientViewDTO
{
    public Guid AssignmentID { get; set; }
    public string TitleEN { get; set; } = string.Empty;
    public string TitleVN { get; set; } = string.Empty;
    public string DescriptionEN { get; set; } = string.Empty;
    public string DescriptionVN { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public int EstimatedTime { get; set; }
    public Guid CreateBy { get; set; }
    public Guid UpdateBy { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
}
