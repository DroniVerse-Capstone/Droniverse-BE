using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class CreateVRSimulatorRequestDTO
{
    public VRSimulatorType Type { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int EstimatedTime { get; set; }
}
