namespace Droniverse.Academy.Application.DTO.Response;

public class UserLevelResponse
{
    public LevelMiniResponse Level { get; set; } = null!;
    public DroneMiniResponse Drone { get; set; } = null!;
}