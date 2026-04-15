

namespace Droniverse.Shared.DTOs
{
    public record GetClubParticipantsResponse
    {
        public required List<Guid> participantIds { get; set; }
    }
}
