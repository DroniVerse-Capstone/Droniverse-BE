using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class VRSimulatorClientViewDTO
{
    public Guid VRSimulatorID { get; set; }
    public VRSimulatorType Type { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public int EstimatedTime { get; set; }
    public DateTime CreateAt { get; set; }
    public SimpleUserReponse? Creator { get; set; }
    public DateTime UpdateAt { get; set; }
    public SimpleUserReponse? Updater { get; set; }
}
