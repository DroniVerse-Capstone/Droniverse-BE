using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class CreateWebSimulatorRequestDTO
{
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public WebSimulatorType Type { get; set; }
    public string ObjectivesVN { get; set; } = null!;
    public string ObjectivesEN { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int EstimatedTime { get; set; }
}
