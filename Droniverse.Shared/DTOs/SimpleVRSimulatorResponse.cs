using Droniverse.Shared.Enums;

namespace Droniverse.Shared.DTOs
{
    public record SimpleVRSimulatorResponse
    {
        public Guid VRSimulatorId { get; set; }
        public required string TitleVN { get; set; }
        public required string TitleEN { get; set; }
        public VRSimulatorType Type { get; set; }
    }
}
