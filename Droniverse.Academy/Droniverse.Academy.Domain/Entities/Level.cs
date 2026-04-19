namespace Droniverse.Academy.Domain.Entities;

public class Level
{
    public Guid LevelID { get; set; }
    public Guid DroneID { get; set; }
    public int LevelNumber { get; set; }
    public string Name { get; set; }

    public Drone Drone { get; set; }
    public ICollection<LevelCourseRequirement> LevelCourseRequirements { get; set; }
    public ICollection<UserLevel> UserLevels { get; set; }
}
