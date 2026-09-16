namespace Droniverse.Shared.DTOs
{
    public record SimpleLevelResponse
    {
        public Guid LevelId { get; set; }
        public required string Name { get; set; }
        public int LevelNumber { get; set; }
        public required SimpleDroneResponse DroneInfo { get; set; }
    }
}
