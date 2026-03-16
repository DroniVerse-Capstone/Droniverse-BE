namespace Droniverse.Academy.Application.DTO.Response;

public class ModuleClientViewDTO
{
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int ModuleNumber { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
}
