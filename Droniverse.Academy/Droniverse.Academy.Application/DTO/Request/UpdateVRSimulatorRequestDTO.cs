namespace Droniverse.Academy.Application.DTO.Request;

public class UpdateVRSimulatorRequestDTO
{
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int EstimatedTime { get; set; }
}
