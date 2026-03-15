namespace Droniverse.Academy.Application.DTO.Request;

public class UpdateModuleRequestDTO
{
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int ModuleNumber { get; set; }
}
