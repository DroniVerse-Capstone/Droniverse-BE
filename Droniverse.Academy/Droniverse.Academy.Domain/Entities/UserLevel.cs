namespace Droniverse.Academy.Domain.Entities;

public class UserLevel
{
    public Guid UserLevelID { get; set; }
    public Guid UserID { get; set; }
    public Guid LevelID { get; set; }
    public DateTime? AchievedAt { get; set; }

    public Level Level { get; set; }
}
