namespace Droniverse.Academy.Application.DTO.Request;

public class CreateTheoryRequestDTO
{
    public Guid LessonID { get; set; }
    public string ContentVN { get; set; } = null!;
    public string ContentEN { get; set; } = null!;
    public int EstimatedTime { get; set; }
}
