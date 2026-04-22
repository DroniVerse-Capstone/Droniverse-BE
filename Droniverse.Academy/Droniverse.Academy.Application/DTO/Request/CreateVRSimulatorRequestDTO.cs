namespace Droniverse.Academy.Application.DTO.Request;

public class CreateVRSimulatorRequestDTO
{
    public Guid ModuleID { get; set; }
    public int? OrderIndex { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int EstimatedTime { get; set; }
}
