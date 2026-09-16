namespace Droniverse.Shared.DTOs.Response;

public class UserLevelResponseDto
{
    public Guid UserID { get; set; }
    public LevelMiniResponseDto Level { get; set; } = null!;
    public DroneMiniResponseDto Drone { get; set; } = null!;
}

public class DroneMiniResponseDto
{
    public Guid DroneID { get; set; }
    public string Name { get; set; }
    public string ImgURL { get; set; }

}

public class LevelMiniResponseDto
{
    public Guid LevelID { get; set; }
    public int LevelNumber { get; set; }
    public string Name { get; set; }
}