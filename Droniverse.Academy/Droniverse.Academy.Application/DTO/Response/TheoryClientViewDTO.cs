namespace Droniverse.Academy.Application.DTO.Response;

public class TheoryClientViewDTO
{
    public Guid TheoryID { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public string ContentVN { get; set; } = null!;
    public string ContentEN { get; set; } = null!;
    public int EstimatedTime { get; set; }
    public DateTime CreateAt { get; set; }
    public Guid CreateBy { get; set; }
    public string? Creator { get; set; }
    public DateTime UpdateAt { get; set; }
    public Guid UpdateBy { get; set; }
    public string? Updater { get; set; }
}
