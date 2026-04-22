using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public class WebSimulatorClientViewDTO
{
    public Guid WebSimulatorID { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string ObjectivesVN { get; set; } = null!;
    public string ObjectivesEN { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int EstimatedTime { get; set; }
    public DateTime CreateAt { get; set; }
    public SimpleUserReponse? Creator { get; set; }
    public DateTime UpdateAt { get; set; }
    public SimpleUserReponse? Updater { get; set; }
}
