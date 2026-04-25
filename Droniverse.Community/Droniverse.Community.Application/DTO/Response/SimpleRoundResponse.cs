using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.DTO.Response
{
    public record SimpleRoundResponse
    {
        public Guid RoundId { get; init; }
        public required SimpleVRSimulatorResponse VRSimulator { get; set; }
        public int RoundNumber { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public TimeSpan TimeLimit { get; init; }
        public RoundStatus RoundStatus { get; init; }
    }
}
