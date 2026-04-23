namespace Droniverse.Shared.DTOs
{
    public record SimpleVRSimulatorResponse
    {
        public Guid VRSimulatorId { get; set; }
        public required string TitleVN { get; set; }
        public required string TitleEN { get; set; }
    }
}
