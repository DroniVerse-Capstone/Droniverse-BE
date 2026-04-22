namespace Droniverse.Shared.DTOs.Response;

public record LevelMiniResponseDto
{
    public Guid LevelID { get; set; }
    public int LevelNumber { get; set; }
    public string Name { get; set; }
}